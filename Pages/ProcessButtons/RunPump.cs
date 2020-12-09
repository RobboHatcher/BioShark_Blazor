using BioShark_Blazor.Data;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.ComponentModel;

namespace BioShark_Blazor.Pages.ProcessButtons {

    public class RunPumpAutoTrigger : IProcessButton {

        private bool isRunning = false;
        private bool DischargeComplete = false;
        private Machine machine; 
        private SummaryTracker tracker;
        private ADC adc;
        private bool initialMassBuffer = true;
        public double StartMass = 0;
        private bool _fromCycle;


        public DateTime OffTimeStart;

        public bool IsInDischarge{
            set{
                IsInDischarge = value;
                if(!value)
                    RunPumpRunChange?.Invoke();
            }
        }

        public event Action RunPumpRunChange;

        public RunPumpAutoTrigger(Machine _machine, SummaryTracker _tracker, ADC _adc){
            machine = _machine;
            tracker = _tracker;
            adc = _adc;
        }


        public void StartProcess(bool fromCycle){
            isRunning = true;
            _fromCycle = fromCycle;
            Console.WriteLine("From Cycle: " + _fromCycle);
            RunPumpRunChange += EndProcess;
            Task.Run(() => { RunPumpAlgorithm();});

            // Loop; until turned off, keep run pump on
        }

        public void StartProcess() {
            StartProcess(true);
        }

        private void RunPumpAlgorithm(){
            machine.TurnOn((int)Machine.OutputPins.RunPump);

            while(isRunning){
                // If water is up to level sensor
                if(machine.IsLevelSensorOn()){
                    Thread.Sleep(100);

                    // If the run pump is On
                    if(machine.IsOn((int)Machine.OutputPins.RunPump)){
                        machine.TurnOff((int)Machine.OutputPins.RunPump);
                        if(initialMassBuffer) {
                            OffTimeStart = DateTime.Now;
                            Console.WriteLine(OffTimeStart);
                        }
                    }
                    else if (!machine.IsOn((int)Machine.OutputPins.RunPump) && initialMassBuffer){
                        Console.WriteLine("Checking runpump time off... " );
                        if(TimeSpan.Compare(DateTime.Now.Subtract(OffTimeStart), TimeSpan.FromSeconds(Constants.SecondsForRunPumpWait)) > 0){
                            Console.WriteLine(Constants.SecondsForRunPumpWait + " seconds passed");
                            StartMass = adc.ScaledNums[(int)ADC.ReadingTypes.Mass];
                            if(_fromCycle){
                                Console.WriteLine("Turning on misting...");
                                machine.TurnOn((int)Machine.OutputPins.Mist);
                                machine.TurnOn((int)Machine.OutputPins.Blower);
                                machine.TurnOn((int)Machine.OutputPins.Heat);
                                machine.TurnOn((int)Machine.OutputPins.Distribution);
                                machine.TurnOn((int)Machine.OutputPins.MistFan);
                            }
                            initialMassBuffer = false;
                        }
                    }
                } 

                else 
                {
                    if(!machine.IsOn((int)Machine.OutputPins.RunPump)){
                        machine.TurnOn((int)Machine.OutputPins.RunPump);
                    }
                }

                if(tracker.peakPPM < adc.ScaledNums[(int)ADC.ReadingTypes.HPHR]) {
                    tracker.peakPPM = adc.ScaledNums[(int)ADC.ReadingTypes.HPHR];
                    tracker.peakPPMTime = DateTime.Now;
                }

                if(tracker.peakRH < adc.ScaledNums[(int)ADC.ReadingTypes.RH]) {
                    tracker.peakRH = adc.ScaledNums[(int)ADC.ReadingTypes.RH];
                    tracker.peakRHTime = DateTime.Now;
                }
            }
        }

        public void EndProcess(){
            isRunning = false;
            initialMassBuffer = true;
            machine.TurnOff((int)Machine.OutputPins.Mist);
            machine.TurnOff((int)Machine.OutputPins.Blower);
            machine.TurnOff((int)Machine.OutputPins.Heat);
            machine.TurnOff((int)Machine.OutputPins.Distribution);
            machine.FillSensorSwitch -= RunPumpAlgorithm;
            RunPumpRunChange -= EndProcess;
            machine.TurnOff((int)Machine.OutputPins.RunPump);
        }

        public bool GetProcessState(){
            return isRunning;
        }

        public string GetButtonClass(){
            if(!isRunning)
                return "btn btn-secondary btn-block";
            else
                return "btn btn-success btn-block";

        }

        

        
    


    }

}