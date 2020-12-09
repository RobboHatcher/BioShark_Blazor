using BioShark_Blazor.Pages.ProcessButtons;
using static BioShark_Blazor.Constants;
using System.IO;
using System;

namespace BioShark_Blazor.Data{

    public class FileIO{

        // This class had three approach options:
        // 1. Open, write, close a file for every data point
        // 2. Save an array for all of the data points of a cycle, then write the entire array to the file
        // 3. Open, write, close for a set amount of data points.
        private Machine machine;
        private ADC adc;
        private ProcessController controller;
        private StreamWriter writer;
        System.Timers.Timer PeriodicFileWriter;

        private int count = 0;
        private string[] dataLines = new string[Constants.PeriodicFileWriteSeconds];
        

        public void Initialize(Machine _machine, ADC _adc, ProcessController _controller){
            machine = _machine;
            adc = _adc;
            controller = _controller;

            // Start Event triggers in autoCycle.startProcess()
            // Stop event triggers in autoCycle.endProcess()
            _controller.autoCycle.cycleStartEvent += StartFileWriter;
            _controller.autoCycle.cycleStopEvent += StopFileWriter;
        }

        private void StartFileWriter(){
            // Sample every second.
            PeriodicFileWriter = new System.Timers.Timer(1000);
            
            string dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)+"/BioShark Data/";
            // Ensure the folder is there; if it isn't, create it.
            if(!File.Exists(dataFolder)){
                Directory.CreateDirectory(dataFolder);
            }

            // Create a file writer.
            writer = new StreamWriter(dataFolder + GetMFileName(dataFolder,false));
            writer.WriteLine("Time,Mass,HPHR,HPLR,Temp,RH,MassDischarged,Mist,MistFan,RunPump,FillPump,DistFan,HRCat,LRCat,"+
                "Sidekick,DrainPump,Blower,Heater,Catalyst,FillStep,DischargeStep,AerationStep,MaxVal");

            // Now, periodically write to a file.
            PeriodicFileWriter.Enabled = true;
            PeriodicFileWriter.Elapsed += SaveDataPoint;
            PeriodicFileWriter.AutoReset = true;
            PeriodicFileWriter.Start();
        }


        private void StopFileWriter(){
           
            
            // If we have a currently open file writer
            if(writer != null){
                if(writer.BaseStream != null){
                    WriteDataLinesToFile();
                    writer.WriteLine("Summary");
                    writer.WriteLine("");
                    writer.WriteLine(controller.tracker.SummaryString());
                }
                try{
                    PeriodicFileWriter.Stop();
                    PeriodicFileWriter.Close();
                    writer.Close();
                    writer.Dispose();
                }
                catch(Exception ex){
                    Console.WriteLine("File Writer already closed.");
                }
            }

        }

        private void SaveDataPoint(object o, System.Timers.ElapsedEventArgs e){
            try{
                dataLines[count] = FormattedDataLine();
            }
            catch(FormatException ex){
                // The data line could not be pulled; one or more data points were absent 
                Console.WriteLine("File writing problem.");

                // Go ahead and just write the date for a line.
                dataLines[count] = DateTime.Now.ToString("HH:mm:ss");
            }

            if(count < Constants.PeriodicFileWriteSeconds - 1 )
                count++;

            else{
                // If we hit the limit or we stop the file writer, we want to write the data lines to the file.
                WriteDataLinesToFile();
            }


        }


        private string GetMFileName(string DataFolder, bool isExtern)
        {
            //Finds out what the current file name should be in a given folder. If local storage, adds "L" to the end of it
            string filename = "";

            //Each File has the format YYYY-MM-DD(i).csv or YYYY-MM-DD(iL).csv, i is the index of how many files have been made that day.
            string currDate = DateTime.Now.ToString(Constants.FileNameFormat);
            var FileList =  Directory.GetFiles(DataFolder);

            Console.WriteLine("Filenames:");
            int Maxdex = 0;
            foreach(var I in FileList)
            {
                string fileName = I;
                Console.WriteLine(fileName);
                if (fileName.Substring(0,10) == currDate)
                {
                    // Index of period: fileName.IndexOf('.')
                    // Length of substring: index of period - 11
                    int index = Convert.ToInt32(fileName.Substring(11, fileName.IndexOf('.') - 11));
                    if ( index >= Maxdex)
                    {
                        // Set max index to one more than index.
                        Maxdex = index + 1;
                    }
                }
            }


            //Ensures that, if we move the files from local to external, there are no filename conflicts
            //Fail case: Local Cycle run, then files moved, then local cycle run again, then files moved again without renaming/removing them from the drive.
            if(isExtern)
                filename += currDate + '(' + Maxdex + ").csv";
            else
                filename += currDate + '(' + Maxdex + "L).csv";

            Console.WriteLine("Creating file " + filename);
            return filename;
        }

        private void WriteDataLinesToFile(){
            for(int i = 0; i < count; i++){
                writer.WriteLine(dataLines[i]);
            }
            count = 0;
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
                controller.lowRangeOscillator.MaxVal + ',' +
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