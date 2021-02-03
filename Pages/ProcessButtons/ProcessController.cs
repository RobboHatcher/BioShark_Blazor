using BioShark_Blazor.Data;
using System.Collections.Generic;

namespace BioShark_Blazor.Pages.ProcessButtons {

    public class ProcessController{
        // ProcessController Class used to hold all scripted processes. This is utilized by the AutoCycle
        public FillPumpWithSensor fillPump;
        public LROscillator lowRangeOscillator;
        public RunPumpAutoTrigger runPump;
        public DrainPump drainPump;
        public AutoCycle autoCycle;
        
        public SummaryTracker tracker = new SummaryTracker();

        public ProcessController(Machine _machine, ADC _adc){
            fillPump = new FillPumpWithSensor(_machine);
            lowRangeOscillator = new LROscillator(_machine, _adc);
            drainPump = new DrainPump(_machine, _adc);
            runPump = new RunPumpAutoTrigger(_machine, tracker, _adc);
            autoCycle = new AutoCycle(_machine, _adc, 
                new List<IProcessButton> {runPump, lowRangeOscillator, drainPump, fillPump}, tracker);
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