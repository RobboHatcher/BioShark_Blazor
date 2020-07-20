using BioShark_Blazor.Data;
using System.Threading.Tasks;
using System;

namespace BioShark_Blazor.Pages.ProcessButtons {

 
    

    public class FillPumpProcessTrigger : IProcessButton {
        //private delegate MassNotifier;
        private bool isRunning = false;
        private Machine machine;
        

        //public event MassNotifier notifier;

        public FillPumpProcessTrigger(Machine _machine){
            machine = _machine;

        }


        public async void StartProcess(){
            
            // Don't start if we are already full
            
            if(!machine.isLevelSensorOn())
            {
                isRunning = true;
                
                machine.TurnOn((int)Machine.OutputPins.FillPump);
                machine.FillSensorSwitch += EndProcess;
                await machine.FillTank();
            }
        }

        public void EndProcess(){
            isRunning = false;
            machine.TurnOff((int)Machine.OutputPins.FillPump);
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