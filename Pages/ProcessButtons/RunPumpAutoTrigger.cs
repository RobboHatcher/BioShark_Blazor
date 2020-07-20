using BioShark_Blazor.Data;
using System.Threading.Tasks;
using System.Threading;

namespace BioShark_Blazor.Pages.ProcessButtons {

    public class RunPumpAutoTrigger : IProcessButton {

        private bool isRunning = false;
        private bool DischargeComplete = false;
        private Machine machine;
        
        public event Running RunPumpRunChange;

        public RunPumpAutoTrigger(Machine _machine){
            machine = _machine;
        }


        public void StartProcess(){
            isRunning = true;
            machine.TurnOn((int)Machine.OutputPins.Mist);
            machine.TurnOn((int)Machine.OutputPins.Blower);
            machine.TurnOn((int)Machine.OutputPins.Heat);
            machine.TurnOn((int)Machine.OutputPins.Distribution);
            Task.Run(async () => {
                while(!DischargeComplete){
                    machine.TurnOn((int)Machine.OutputPins.RunPump);
                    await machine.FillTank();
                    machine.TurnOff((int)Machine.OutputPins.RunPump);
                    Thread.Sleep(1000);
                }
            });

            // Loop; until turned off, keep run pump on
        }

        public void EndProcess(){
            isRunning = false;
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