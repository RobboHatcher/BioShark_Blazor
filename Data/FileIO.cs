using BioShark_Blazor.Pages.ProcessButtons;
using System.IO;
using System;

namespace BioShark_Blazor.Data{

    public class FileIO{
        private Machine machine;
        private ADC adc;
        private ProcessController controller;
        private StreamWriter writer;
        System.Timers.Timer PeriodicFileWriter;
        public void Initialize(Machine _machine, ADC _adc, ProcessController _controller){
            machine = _machine;
            adc = _adc;
            controller = _controller;
            _controller.autoCycle.cycleStartEvent += StartFileWriter;
            _controller.autoCycle.cycleStopEvent += StopFileWriter;
        }

        private void StartFileWriter(){
            PeriodicFileWriter = new System.Timers.Timer(1000);
            
            writer = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)+"/BioShark Data/CycleTrackFile.csv");
            writer.WriteLine("Time,Mass,HPHR,HPLR,Temp,RH,MassDischarged,Mist,MistFan,RunPump,FillPump,DistFan,HRCat,LRCat,"+
                "Sidekick,DrainPump,Blower,Heater,Catalyst,FillStep,DischargeStep,AerationStep");
            PeriodicFileWriter.Enabled = true;
            PeriodicFileWriter.Elapsed += WriteToFile;
            PeriodicFileWriter.AutoReset = true;
            PeriodicFileWriter.Start();
        }


        private void StopFileWriter(){
            PeriodicFileWriter.Stop();
            PeriodicFileWriter.Close();
            writer.Close();
            writer.Dispose();

        }
        
        private void WriteToFile(object o, System.Timers.ElapsedEventArgs e){
            writer.WriteLine(FormattedDataLine());
        }

        private string FormattedDataLine(){
            string buildString = "";
            buildString += DateTime.Now.ToString("HH:mm:ss") + ',' +
                adc.ScaledNums[(int)ADC.ReadingTypes.Mass].ToString() + ',' +
                adc.ScaledNums[(int)ADC.ReadingTypes.HPHR] + ',' +
                adc.ScaledNums[(int)ADC.ReadingTypes.HPLR] + ',' +
                adc.ScaledNums[(int)ADC.ReadingTypes.Temp] + ',' +
                adc.ScaledNums[(int)ADC.ReadingTypes.RH] + ',' +
                controller.autoCycle.MassDischarged + ',' +
                machine.IsOn((int)Machine.OutputPins.Mist).ToString() + ',' +
                machine.IsOn((int)Machine.OutputPins.MistFan).ToString() + ',' +
                machine.IsOn((int)Machine.OutputPins.RunPump).ToString() + ',' +
                machine.IsOn((int)Machine.OutputPins.FillPump).ToString() + ',' +
                machine.IsOn((int)Machine.OutputPins.Distribution).ToString() + ',' +
                machine.IsOn((int)Machine.OutputPins.HRCat).ToString() + ',' +
                machine.IsOn((int)Machine.OutputPins.LRCat).ToString() + ',' +
                machine.IsOn((int)Machine.OutputPins.Sidekick).ToString() + ',' +
                machine.IsOn((int)Machine.OutputPins.Drainpump).ToString() + ',' +
                machine.IsOn((int)Machine.OutputPins.Blower).ToString() + ',' +
                machine.IsOn((int)Machine.OutputPins.Heat).ToString() + ',' +
                machine.IsOn((int)Machine.OutputPins.Cat).ToString() + ',' +
                controller.fillPump.GetProcessState().ToString() + ',' +
                controller.runPump.GetProcessState().ToString() + ',' +
                controller.lowRangeOscillator.GetProcessState().ToString();


            return buildString;
        }
    }
}