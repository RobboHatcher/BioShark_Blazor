using BioShark_Blazor.Data;
using System.Threading.Tasks;
using System;

namespace BioShark_Blazor.Pages.ProcessButtons {
    public class FillPumpWithSensor : IProcessButton {
        private bool isRunning = false;
        private Machine machine;

        public FillPumpWithSensor(Machine _machine){
            machine = _machine;
        }


        public async void StartProcess(bool fromCycle){
            
            // Don't start if we are already full

            if(!machine.IsLevelSensorOn())
            {
                isRunning = true;
                
                machine.TurnOn((int)Machine.OutputPins.FillPump);
                machine.FillSensorSwitch += EndProcess;
                await machine.FillTank();
            }
        }
        public void StartProcess(){
            StartProcess(true);
        }

        public void EndProcess(){
            isRunning = false;
            machine.TurnOff((int)Machine.OutputPins.FillPump);
            machine.FillSensorSwitch -= EndProcess;
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