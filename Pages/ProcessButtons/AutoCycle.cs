using BioShark_Blazor.Data;
using System.Timers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BioShark_Blazor.Pages.ProcessButtons {

    public class AutoCycle : IProcessButton {

        public enum CycleButtons {RunPump, LROsc, DrainPump, FillPump}
        private bool isRunning = false;
        private Machine machine;
        private ADC adc;
        private CycleData _data;
        private double MassDischarged = 0, StartMass = 0;
        private DateTime cycleStart;
        private System.Timers.Timer CycleSideKick;
        private List<IProcessButton> buttons;

        public AutoCycle(Machine _machine, ADC _adc, List<IProcessButton> _buttons) {
            machine = _machine;
            adc = _adc;
            buttons = _buttons;
        }

        public void EndProcess(){
            try{
                CycleSideKick.Dispose();
            }
            catch(Exception ex){
                
            }
            isRunning = false;
            machine.TurnAllOff();
            machine.TurnOn((int)Machine.OutputPins.LRCat);
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
            CycleSideKick = new System.Timers.Timer(Constants.SidekickMS);
            CycleSideKick.Elapsed += FillMister;
            CycleSideKick.AutoReset = false;
            CycleSideKick.Start();
        }

        private async void FillMister(object source, ElapsedEventArgs e){
            cycleStart = DateTime.Now;
            Console.WriteLine("Filling...");
            machine.TurnOff((int)Machine.OutputPins.Sidekick);
            buttons[(int)CycleButtons.FillPump].StartProcess();
            // For now, wait
            await Task.Run(()=> { 
                while(buttons[(int)CycleButtons.FillPump].GetProcessState())
                {
                     Thread.Sleep(1000);
                }
            }); // While is running, check once per second to see if it stops.
            StartMass = adc.ScaledNums[(int)ADC.ReadingTypes.Mass];
            StartDischarge();
            // wait until the ppm is at the desired point
        }

        private async void StartDischarge(){
            Console.WriteLine("Discharging...");
            double TargetMass = Constants.TestingTargetMass;

            buttons[(int)CycleButtons.RunPump].StartProcess();
            await Task.Run(()=> { 
                while(MassDischarged < TargetMass){
                    MassDischarged = StartMass - adc.ScaledNums[(int)ADC.ReadingTypes.Mass];
                    Thread.Sleep(500);
                } // Wait until mass above target
                while(MassDischarged >= TargetMass){
                    if(adc.ScaledNums[(int)ADC.ReadingTypes.HPHR] > Constants.TargetAmt)
                    {
                        Console.WriteLine("PPM Target Reached");
                        buttons[(int)CycleButtons.RunPump].EndProcess();
                        StartHold();
                    }
                    else if(MassDischarged > TargetMass * Constants.ExtraMassFactor){
                        Console.WriteLine("Ending Cycle Early, going to start");
                        EndProcess();

                    }

                    Thread.Sleep(500);
                }

            });
        }


        private void StartHold(){
            Console.WriteLine("Hold Step: " + DateTime.Now);

           

            machine.TurnOn((int)Machine.OutputPins.Distribution);
            buttons[(int)CycleButtons.DrainPump].StartProcess();
            Task.Run(()=>{
                while((DateTime.Now.Subtract(cycleStart) < TimeSpan.FromMinutes(10))){
                    Thread.Sleep(1000);
                    if(!((DrainPump)buttons[(int)CycleButtons.DrainPump]).isRunning) {
                        CycleSideKick = new System.Timers.Timer(Constants.SidekickMS);
                        CycleSideKick.Elapsed += StopSideKick;
                        CycleSideKick.AutoReset = false;
                        CycleSideKick.Start();
                    }
                }
                StartAeration();
            });
        }

        private void StopSideKick(object source, ElapsedEventArgs e){
            machine.TurnOff((int)Machine.OutputPins.Sidekick);
            buttons[(int)CycleButtons.DrainPump].StartProcess();
        }

        private void StartAeration(){
            machine.TurnOff((int)Machine.OutputPins.Blower);
            machine.TurnOff((int)Machine.OutputPins.Heat);
            machine.TurnOff((int)Machine.OutputPins.Mist);
            machine.TurnOff((int)Machine.OutputPins.MistFan);
            buttons[(int)CycleButtons.LROsc].StartProcess();

            while(((LROscillator)buttons[(int)CycleButtons.LROsc]).isRunning){ Thread.Sleep(1000); }

            EndProcess();
        }
        
    
    
    }
}