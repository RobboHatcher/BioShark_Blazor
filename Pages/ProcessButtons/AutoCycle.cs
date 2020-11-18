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
        SummaryTracker tracker;

        public double MassDischarged = 0;
        private double StartMass = 0;
        public TimeSpan estTimeLeft = new TimeSpan(0,0,0);
        private DateTime cycleStart;
        private DateTime estCycleEnd = DateTime.Now;

        private System.Timers.Timer CycleSideKick;
        private bool secondDrainCompleteFlag = false;
        private List<IProcessButton> cycleProcesses;
        public event Action cycleStartEvent;
        public event Action cycleStopEvent;


        public AutoCycle(Machine _machine, ADC _adc, List<IProcessButton> _buttons, SummaryTracker _tracker) {
            machine = _machine;
            adc = _adc;
            cycleProcesses = _buttons;
            tracker = _tracker;
            adc.OnAverageValues += UpdateEstTime;

        }

        public void StartProcess(bool fromCycle){



            cycleStartEvent?.Invoke();
            isRunning = true;
            machine.TurnOn((int)Machine.OutputPins.LRCat);
            
            RunCycle();
        }

        public void StartProcess(){
            StartProcess(true);
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
            
            tracker.massDisch = MassDischarged;
            tracker.endMass = adc.ScaledNums[(int)ADC.ReadingTypes.Mass];
            tracker.massChange = tracker.begMass - tracker.endMass;
            tracker.endRH = adc.ScaledNums[(int)ADC.ReadingTypes.RH];
            tracker.endTemp = adc.ScaledNums[(int)ADC.ReadingTypes.Temp];
            tracker.endTime = DateTime.Now;
            FillSensorSwitch -= StartDischarge;
            machine.TurnAllOff();
            cycleStopEvent?.Invoke();
            secondDrainCompleteFlag = false;
            cycleProcesses[2].StartProcess(); //Start a drain 
        }

        public void UpdateEstTime(){
            estTimeLeft = TimeSpan.FromSeconds(60 * (Math.Log(machine.targetMass, 1.15)* 2) - 30);
            
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
            estCycleEnd = DateTime.Now.Add(estTimeLeft);
            machine.TurnOn((int)Machine.OutputPins.Sidekick);

            
            tracker.roomVolume = BioShark_Blazor.Pages.Index.roomSize;
            tracker.begMass = adc.ScaledNums[(int)ADC.ReadingTypes.Mass];
            tracker.begRH = adc.ScaledNums[(int)ADC.ReadingTypes.RH];
            tracker.begTemp = adc.ScaledNums[(int)ADC.ReadingTypes.Temp];
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
            tracker.startTime = cycleStart;

            machine.FillSensorSwitch -= StartDischarge;
            
            Console.WriteLine("Discharging... Start mass @ " + Math.Round(StartMass,2));
            double TargetMass = machine.targetMass;

            cycleProcesses[(int)processEnum.RunPump].StartProcess(true);

            

            await Task.Run(()=> { 
                // Wait until the run pump saves the start mass
                while(((RunPumpAutoTrigger)(cycleProcesses[(int)processEnum.RunPump])).StartMass <= 0){} 
                
                StartMass = ((RunPumpAutoTrigger)(cycleProcesses[(int)processEnum.RunPump])).StartMass;
                while(MassDischarged < TargetMass && isRunning && StartMass > 0){
                    MassDischarged = StartMass - adc.ScaledNums[(int)ADC.ReadingTypes.Mass];
                    Thread.Sleep(500);
                } // Wait until mass above target
                while(MassDischarged >= TargetMass && isRunning){
                    MassDischarged = StartMass - adc.ScaledNums[(int)ADC.ReadingTypes.Mass];
                    if(adc.ScaledNums[(int)ADC.ReadingTypes.HPHR] > Constants.TargetAmt)
                    {
                        Console.WriteLine("PPM Target Reached");
                        cycleProcesses[(int)processEnum.RunPump].EndProcess();
                        StartHold();
                        break;
                    }
                    else if(MassDischarged > TargetMass * Constants.ExtraMassFactor){
                        Console.WriteLine("Ending Cycle Early, going to start");
                        EndProcess();
                        break;

                    }

                    Thread.Sleep(500);
                }

            });
        }


        private void StartHold(){
            Console.WriteLine("Hold Step: " + DateTime.Now);
            machine.TurnOn((int)Machine.OutputPins.Distribution);
            machine.TurnOn((int)Machine.OutputPins.Blower);
            cycleProcesses[(int)processEnum.DrainPump].StartProcess();
            Task.Run(()=>{
                while((DateTime.Now.Subtract(cycleStart) < TimeSpan.FromMinutes(10))){
                    Thread.Sleep(1000);
                    if(!((DrainPump)cycleProcesses[(int)processEnum.DrainPump]).isRunning && !secondDrainCompleteFlag) {
                        CycleSideKick = new System.Timers.Timer(Constants.SidekickMS);
                        CycleSideKick.Elapsed += StopSideKick;
                        CycleSideKick.AutoReset = false;
                        secondDrainCompleteFlag = true;
                        machine.TurnOn((int)Machine.OutputPins.Sidekick);
                        CycleSideKick.Start();
                    }
                }
                StartAeration();
            });
        }

        private void StopSideKick(object source, ElapsedEventArgs e){
            machine.TurnOff((int)Machine.OutputPins.Sidekick);
            cycleProcesses[(int)processEnum.DrainPump].StartProcess();
            CycleSideKick.Stop();
            CycleSideKick.Dispose();
        }

        private void StartAeration(){
            machine.TurnOff((int)Machine.OutputPins.Blower);
            machine.TurnOff((int)Machine.OutputPins.Heat);
            machine.TurnOff((int)Machine.OutputPins.Mist);
            machine.TurnOff((int)Machine.OutputPins.MistFan);
            
            machine.TurnOn((int)Machine.OutputPins.Cat);
            cycleProcesses[(int)processEnum.LROsc].StartProcess();
            while(((LROscillator)cycleProcesses[(int)processEnum.LROsc]).isRunning){ Thread.Sleep(1000); }

            EndProcess();
        }
        
    
    
    }
}