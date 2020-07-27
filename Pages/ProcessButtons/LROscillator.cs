using BioShark_Blazor.Data;
namespace BioShark_Blazor.Pages.ProcessButtons {

    public class LROscillator : IProcessButton {

        private bool isRunning = false;
        private Machine machine;

        public LROscillator(Machine _machine) {
            machine = _machine;
        }

        public void StartProcess(){
             isRunning = true;
             machine.TurnOn((int)Machine.OutputPins.LRCat);
        }
        public void EndProcess(){
            isRunning = false;
            machine.TurnOff((int)Machine.OutputPins.LRCat);
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
    
    
    }
}