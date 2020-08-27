using BioShark_Blazor.Data;
using System.Collections.Generic;

namespace BioShark_Blazor.Pages.ProcessButtons {

    public class ProcessController{
        public FillPumpWithSensor fillPump;
        public LROscillator lowRangeOscillator;
        public RunPumpAutoTrigger runPump;
        public DrainPump drainPump;
        public AutoCycle autoCycle;
        
        public ProcessController(Machine _machine, ADC _adc){
            fillPump = new FillPumpWithSensor(_machine);
            lowRangeOscillator = new LROscillator(_machine, _adc);
            drainPump = new DrainPump(_machine, _adc);
            runPump = new RunPumpAutoTrigger(_machine);
            autoCycle = new AutoCycle(_machine, _adc, 
                new List<IProcessButton> {runPump, lowRangeOscillator, drainPump, fillPump});
        }

        public void StopAllProcesses(){
            fillPump.EndProcess();
            lowRangeOscillator.EndProcess();
            runPump.EndProcess();
            drainPump.EndProcess();
            autoCycle.EndProcess();
        }

    }
}