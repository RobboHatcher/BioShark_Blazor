using BioShark_Blazor.Data;
namespace BioShark_Blazor.Pages.ProcessButtons {

    public class AutoCycleProcessTrigger : IProcessButton {

        private bool isRunning = false;
        private Machine machine;
        private CycleData _data;
        public AutoCycleProcessTrigger(Machine _machine) {
            machine = _machine;

        }

        public void StartProcess(){
            isRunning = true;
            _data = new CycleData();
            

        }


        public void EndProcess(){
            isRunning = false;

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