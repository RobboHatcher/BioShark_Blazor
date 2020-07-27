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
        
        public bool IsInDischarge{
            set{
                IsInDischarge = value;
                if(!value)
                    RunPumpRunChange?.Invoke();
            }
        }

        public event Action RunPumpRunChange;

        public RunPumpAutoTrigger(Machine _machine){
            machine = _machine;
        }


        public void StartProcess(){
            isRunning = true;
            machine.TurnOn((int)Machine.OutputPins.Mist);
            machine.TurnOn((int)Machine.OutputPins.Blower);
            machine.TurnOn((int)Machine.OutputPins.Heat);
            machine.TurnOn((int)Machine.OutputPins.Distribution);
            machine.TurnOn((int)Machine.OutputPins.MistFan);
            RunPumpRunChange += EndProcess;
            machine.FillSensorSwitch += RunPumpAlgorithm;
            Task.Run(() => { RunPumpAlgorithm();});

            // Loop; until turned off, keep run pump on
        }

        private void RunPumpAlgorithm(){
            machine.TurnOn((int)Machine.OutputPins.RunPump);
            while(isRunning){
                if(machine.IsLevelSensorOn()){
                    Thread.Sleep(100);
                    if(machine.IsLevelSensorOn()){
                        machine.TurnOff((int)Machine.OutputPins.RunPump);
                    }
                }
                
                else 
                {
                    if(!machine.IsOn((int)Machine.OutputPins.RunPump)){
                        machine.TurnOn((int)Machine.OutputPins.RunPump);
                    }
                }

            }
        }

        public void EndProcess(){
            isRunning = false;
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