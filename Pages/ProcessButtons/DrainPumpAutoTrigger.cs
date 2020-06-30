using BioShark_Blazor.Data;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace BioShark_Blazor.Pages.ProcessButtons {

    public delegate void Running();
    
    public class DrainPumpAutoTrigger : IProcessButton {
        public event Running DrainRunChange;
        private bool isRunning = false;
        private Machine machine;
        private ADC adc;
        private double prevMassReading = 0;
        private double currMassReading = 0;

        
        public DrainPumpAutoTrigger(ref Machine _machine, ADC _adc) {
            machine = _machine;
            adc = _adc;
        }

        public void StartProcess(){
            isRunning = true;
            machine.TurnOn((int)Machine.OutputPins.Drainpump);
            DrainRunChange?.Invoke();
            Task.Run(() => { 
                while(!isTankEmpty() && isRunning) {}
                
            });


        }
        public void EndProcess(){
            isRunning = false;
            machine.TurnOff((int)Machine.OutputPins.Drainpump);
            Thread.Sleep(250);
            DrainRunChange?.Invoke();
        }

        public string GetButtonClass(){
            if(!isRunning)
                return "btn btn-secondary btn-block";
            else
                return "btn btn-success btn-block";
        }
        public bool GetProcessState(){
            return isRunning;
        }


        private bool isTankEmpty() {
            bool empty = false;
            // Drain until the mass value is fairly constant
            // Tolerance: must be within the same mass for 3 seconds
            prevMassReading = currMassReading;
            Thread.Sleep(1000);
            currMassReading = adc.ScaledNums[(int)ADC.ReadingTypes.Mass];
            Console.WriteLine(currMassReading + ", " + prevMassReading);
            if(Math.Abs(currMassReading - prevMassReading) <= Constants.DrainToleranceVal){
                EndProcess();
                return true;
            }

            return false;
            
        }
    
    }
}