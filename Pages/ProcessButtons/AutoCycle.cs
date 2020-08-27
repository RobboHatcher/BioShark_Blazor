using BioShark_Blazor.Data;
using System.Timers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BioShark_Blazor.Pages.ProcessButtons {

    public class AutoCycle : IProcessButton {

        protected enum processEnum {RunPump, LROsc, DrainPump, FillPump}

        private Machine machine;
        private ADC adc;

        private bool isRunning = false;

        private CycleData _data;

        private double MassDischarged = 0, StartMass = 0;

        private DateTime cycleStart;

        private System.Timers.Timer CycleSideKick;

        private List<IProcessButton> cycleProcesses;
        public event Action cycleStartEvent;
        public event Action cycleStopEvent;
        public AutoCycle(Machine _machine, ADC _adc, List<IProcessButton> _buttons) {
            machine = _machine;
            adc = _adc;
            cycleProcesses = _buttons;
        }

        public void StartProcess(){

            cycleStartEvent?.Invoke();
            isRunning = true;
            machine.TurnOn((int)Machine.OutputPins.LRCat);
            
            _data = new CycleData();
            RunCycle();
        }

        public void EndProcess(){
            try{
                CycleSideKick.Dispose();
            }
            catch(Exception ex){
                
            }
            isRunning = false;
            
            foreach(var process in cycleProcesses){
                process.EndProcess();
            }
            

            machine.TurnAllOff();
            cycleStopEvent?.Invoke();
            cycleProcesses[2].StartProcess(); //Start a drain 
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

        private void RunCycle(){
            machine.TurnOn((int)Machine.OutputPins.Sidekick);
            CycleSideKick = new System.Timers.Timer(Constants.SidekickMS);
            CycleSideKick.Elapsed += FillMister;
            CycleSideKick.AutoReset = false;
            CycleSideKick.Start();
        }

        private async void FillMister(object source, ElapsedEventArgs e){
            // Step one of the cycle: Fill the mister until the level sensor triggers.

            machine.TurnOff((int)Machine.OutputPins.Sidekick); // Sidekick was running at the beginning of the cycle, so turn it off
            

            Console.WriteLine("Filling...");
            cycleProcesses[(int)processEnum.FillPump].StartProcess();

            // When the capacitive sensor triggers, start the discharge step.
            machine.FillSensorSwitch += StartDischarge; 
        }

        private async void StartDischarge(){
            cycleStart = DateTime.Now; // Save the start time for the hold step
            machine.FillSensorSwitch -= StartDischarge;
            StartMass = adc.ScaledNums[(int)ADC.ReadingTypes.Mass];
            Console.WriteLine("Discharging...");
            double TargetMass = machine.targetMass;

            cycleProcesses[(int)processEnum.RunPump].StartProcess();
            await Task.Run(()=> { 
                while(MassDischarged < TargetMass){
                    MassDischarged = StartMass - adc.ScaledNums[(int)ADC.ReadingTypes.Mass];
                    Thread.Sleep(500);
                } // Wait until mass above target
                while(MassDischarged >= TargetMass){
                    MassDischarged = StartMass - adc.ScaledNums[(int)ADC.ReadingTypes.Mass];
                    if(adc.ScaledNums[(int)ADC.ReadingTypes.HPHR] > Constants.TargetAmt)
                    {
                        Console.WriteLine("PPM Target Reached");
                        cycleProcesses[(int)processEnum.RunPump].EndProcess();
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
            cycleProcesses[(int)processEnum.DrainPump].StartProcess();
            Task.Run(()=>{
                while((DateTime.Now.Subtract(cycleStart) < TimeSpan.FromMinutes(10))){
                    Thread.Sleep(1000);
                    if(!((DrainPump)cycleProcesses[(int)processEnum.DrainPump]).isRunning) {
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
            cycleProcesses[(int)processEnum.DrainPump].StartProcess();
        }

        private void StartAeration(){
            machine.TurnOff((int)Machine.OutputPins.Blower);
            machine.TurnOff((int)Machine.OutputPins.Heat);
            machine.TurnOff((int)Machine.OutputPins.Mist);
            machine.TurnOff((int)Machine.OutputPins.MistFan);
            cycleProcesses[(int)processEnum.LROsc].StartProcess();

            while(((LROscillator)cycleProcesses[(int)processEnum.LROsc]).isRunning){ Thread.Sleep(1000); }

            EndProcess();
        }
        
    
    
    }
}