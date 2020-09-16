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
            string dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)+"/BioShark Data/");
            writer = new StreamWriter(dataFolder + GetMFileName(dataFolder,false));
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

        private string GetMFileName(string DataFolder, bool isExtern)
        {
            //Finds out what the current file name should be in a given folder. If local storage, adds "L" to the end of it
            string filename = "";

            //Each File has the format YYYY-MM-DD(i).csv or YYYY-MM-DD(iL).csv, i is the index of how many files have been made that day.
            string currDate = DateTime.Now.ToString(FileNameFormat);
            var FileList =  GetFiles(DataFolder);

            WriteLine("Filenames:");
            int Maxdex = 0;
            foreach(var I in FileList)
            {
                string fileName = I;
                WriteLine(fileName);
                if (fileName.Substring(0,10) == currDate.Substring(0,10))
                {
                    int index = Convert.ToInt32(fileName.Substring(11, 1));
                    if ( index > Maxdex)
                    {
                        Maxdex = index;
                    }
                }
            }

            Maxdex += 1;

            //Ensures that, if we move the files from local to external, there are no filename conflicts
            //Fail case: Local Cycle run, then files moved, then local cycle run again, then files moved again without renaming/removing them from the drive.
            if(isExtern)
                filename += currDate + '(' + Maxdex + ").csv";
            else
                filename += currDate + '(' + Maxdex + "L).csv";

            WriteLine("Creating file " + filename);
            return filename;
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