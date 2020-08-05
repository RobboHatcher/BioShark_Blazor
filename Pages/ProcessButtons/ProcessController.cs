using BioShark_Blazor.Data;


namespace BioShark_Blazor.Pages.ProcessButtons {

    public class ProcessController{
        private FillPumpWithSensor fillPump;
        private LROscillator lowRangeOscillator;
        private RunPumpAutoTrigger runPump;
        private DrainPump drainPump;
        public ProcessController(Machine _machine, ADC _adc){
            fillPump = new FillPumpWithSensor(_machine);
            lowRangeOscillator = new LROscillator(_machine);
            drainPump = new DrainPump(_machine, _adc);
            runPump = new RunPumpAutoTrigger(_machine);
        }

        public void StartDrainPump(){
            drainPump.StartProcess();
        }

        public void EndDrainPump(){
            drainPump.EndProcess();
        }

        public void 

        public void StartLROscillator(){
            lowRangeOscillator.StartProcess();
        }

        public void EndLROscillator(){
            lowRangeOscillator.EndProcess();
        }

        public void StartFillPump(){
            fillPump.StartProcess();
        }

        public void EndFillPump(){
            fillPump.EndProcess();
        }

    }
}