using BioShark_Blazor.Data;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace BioShark_Blazor.Pages.ProcessButtons {

    public class LROscillator : IProcessButton {

        public bool isRunning = false;
        private Machine machine;
        private ADC adc;
        private int PeakCtr = 0;
        private bool safeToEnter = false;
        private double MaxVal = 0;
        public LROscillator(Machine _machine, ADC _adc) {
            machine = _machine;
            adc = _adc;

        }

        public void StartProcess(){
            isRunning = true;
            machine.TurnOn((int)Machine.OutputPins.LRCat);
            // Every quarter second: check for safe to enter
            Task.Run(()=>{
                while(!safeToEnter){
                    if(adc.ScaledNums[(int)ADC.ReadingTypes.HPLR] > Constants.OscillationConstant)
                    {
                        machine.TurnOn((int)Machine.OutputPins.LRCat);
                        if(adc.ScaledNums[(int)ADC.ReadingTypes.HPLR] > MaxVal) MaxVal = adc.ScaledNums[(int)ADC.ReadingTypes.HPLR];
                    }

                    else{
                        machine.TurnOff((int)Machine.OutputPins.LRCat);
                        if(MaxVal <= Constants.PeakConstant && MaxVal > Constants.OscillationConstant)
                        {
                            PeakCtr++;
                            Console.WriteLine("Peaked at " + MaxVal + ". Peak Count : " + PeakCtr);
                        }
                        else if (MaxVal > Constants.PeakConstant)
                        {
                            PeakCtr = 0;
                            Console.WriteLine("Peaks Reset to 0");
                        }
                        MaxVal = Constants.OscillationConstant;
                    }

                    SafetyCheck();

                    Thread.Sleep(250);
                }
            });


        }

        public void EndProcess(){
            isRunning = false;
            safeToEnter = true;
            Thread.Sleep(1000);
            safeToEnter = false;
            machine.TurnOff((int)Machine.OutputPins.LRCat);
        }
        public string GetButtonClass(){
            if(!isRunning)
                return "btn btn-secondary btn-block";
            else
                return "btn btn-success btn-block";
        }

        private void SafetyCheck() {
            if(PeakCtr >= 3){
                EndProcess();
                safeToEnter = true;
            }

            else safeToEnter = false;
        }
        public bool GetProcessState(){
            return isRunning;
        }
    
    
    }
}