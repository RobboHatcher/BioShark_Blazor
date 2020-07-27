using BioShark_Blazor.Data;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace BioShark_Blazor.Pages.ProcessButtons {

    public delegate void Running();
    
    public class DrainPump : IProcessButton {
        public event Running DrainRunChange;
        private bool isRunning = false;
        private Machine machine;
        private ADC adc;
        private double prevMassAvg;
        private double currMassAvg;
        private double currMassReading = 0;

        private double[] drainReadings =  {0,0,0,0,0,0}; // Drain rolling average

        
        public DrainPump(Machine _machine, ADC _adc) {
            machine = _machine;
            adc = _adc;
        }
    
        public void StartProcess(){
            if(!isRunning){
                isRunning = true;
                drainReadings = new double[6]{0,0,0,0,0,0};
                machine.TurnOn((int)Machine.OutputPins.Drainpump);
                DrainRunChange?.Invoke();
                Task.Run(() => { 
                    while(!isTankEmpty() && isRunning) {}
                });
            }
        


        }
        public void EndProcess(){
            if(isRunning){
                isRunning = false;
                machine.TurnOff((int)Machine.OutputPins.Drainpump);
                Thread.Sleep(250);
                DrainRunChange?.Invoke();
            }
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
            currMassReading = adc.ScaledNums[(int)ADC.ReadingTypes.Mass];
            // Drain until the mass value is fairly constant
            // Tolerance: average value must not change by more than 1 gram

            for(int i = drainReadings.Length - 1; i > 0; i--)
            {
                drainReadings[i] = drainReadings[i-1];
            }

            drainReadings[0] = currMassReading;
            
            prevMassAvg = currMassAvg;
            currMassAvg = 0;

            for(int i =0; i < drainReadings.Length; i++)
            {
                currMassAvg += drainReadings[i];
            }

            currMassAvg /= drainReadings.Length;

            Thread.Sleep(1000);
            
            Console.WriteLine(prevMassAvg + ", " + currMassAvg);
            if(Math.Abs(currMassAvg - prevMassAvg) <= Constants.DrainToleranceVal){
                EndProcess();
                return true;
            }

            return false;
            
        }
    
    }
}