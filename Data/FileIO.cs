using BioShark_Blazor.Pages.ProcessButtons;


namespace BioShark_Blazor.Data{

    public class FileIO{
        private Machine machine;
        private ADC adc;
        private ProcessController controller;
        public void Initialize(Machine _machine, ADC _adc, ProcessController _controller){
            machine = _machine;
            adc = _adc;
            controller = _controller;
            _controller.autoCycle.cycleStartEvent += StartFileWriter;
        }

        private void StartFileWriter(){
            System.Timers.Timer PeriodicFileWriter = new System.Timers.Timer(1000);
            PeriodicFileWriter.Enabled = true;
            PeriodicFileWriter.Elapsed += WriteToFile;
        }
        
        private void WriteToFile(object o, System.Timers.ElapsedEventArgs e){
            
        }
    }
}