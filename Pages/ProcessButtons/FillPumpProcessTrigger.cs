using BioShark_Blazor.Data;
using System.Threading.Tasks;

namespace BioShark_Blazor.Pages.ProcessButtons {

 
    public class FillPumpProcessTrigger : IProcessButton {

        private bool isRunning = false;
        private Machine machine;
        
        public event Running FillPumpRun;

        public FillPumpProcessTrigger(Machine _machine){
            machine = _machine;
        }


        public async void StartProcess(){
            isRunning = true;
            machine.TurnOn((int)Machine.OutputPins.FillPump);
            await machine.FillTank();
            EndProcess();
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