using BioShark_Blazor.Data;
using System.Timers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BioShark_Blazor.Pages.ProcessButtons {

    public class AutoCycleProcessTrigger : IProcessButton {

        public enum CycleButtons {RunPump, LROsc, DrainPump, FillPump}
        private bool isRunning = false;
        private Machine machine;
        private ADC adc;
        private CycleData _data;
        private Timer PreCycleSideKick;
        private List<IProcessButton> buttons;

        public AutoCycleProcessTrigger(Machine _machine, ADC _adc, List<IProcessButton> _buttons) {
            machine = _machine;
            adc = _adc;
            buttons = _buttons;
        }

        public void EndProcess(){
            try{
                PreCycleSideKick.Dispose();
            }
            catch(Exception ex){
                
            }
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
        public void StartProcess(){
            isRunning = true;
            machine.TurnOn((int)Machine.OutputPins.LRCat);
            _data = new CycleData();
            RunCycle();
        }


        private void RunCycle(){
            machine.TurnOn((int)Machine.OutputPins.Sidekick);
            PreCycleSideKick = new System.Timers.Timer(Constants.SidekickMS);
            PreCycleSideKick.Elapsed += FillMister;
            PreCycleSideKick.AutoReset = false;
            PreCycleSideKick.Start();
        }

        private async void FillMister(object source, ElapsedEventArgs e){
            machine.TurnOff((int)Machine.OutputPins.Sidekick);
            buttons[(int)CycleButtons.FillPump].StartProcess();
            // For now, wait
            await Task.Run(()=> { 
                while(buttons[(int)CycleButtons.FillPump].GetProcessState())
                {
                     Thread.Sleep(1000);
                }
            }); // While is running, check once per second to see if it stops.

            StartDischarge();

        }

        private async void StartDischarge(){
            buttons[(int)CycleButtons.RunPump].StartProcess();
            await Task.Run(()=> { 
                while(buttons[(int)CycleButtons.FillPump].GetProcessState())
                {
                     Thread.Sleep(1000);
                }
            });
        }
        
    
    
    }
}