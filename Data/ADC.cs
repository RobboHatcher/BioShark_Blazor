using System;
using System.Threading;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;
using System.IO;



namespace BioShark_Blazor.Data {

    
    public class ADC {
        public event Action OnAverageValues;
        public enum ADCOutPins { ResetPin = 22, ConvertStartPin = 24 }
        public enum ADCInPins { BusyPin = 25 }
        GpioController _adcControl;
        SpiDevice spi;


        public enum ReadingTypes { Mass, HPHR, HPLR, RH, Temp }
        
        
        private int NaNCounter = 0;
        private uint SampleNum = 0;
        // For storing the sum of the readings until averaged
        private double[] readSums = {0,0,0,0,0};
        private double[] Avgs = {0,0,0,0,0};
        public double[] ScaledNums = {0,0,0,0,0};


        public ScalingVals _scale {get;set;} = new ScalingVals();
        public List<ADCPin> _inputs;
        public List<ADCPin> _outputs;  
        public double MassStart, MassFilled = 0;
        public bool busyReading = false;
        public bool busyAveraging = false;
        public bool busyCalibrating = false;


        public double[] Readings {get;set;} = {0,0,0,0,0};

        public ADC() {
            _adcControl = new GpioController();
            // Initialize SPI
            InitSPI();


            // Initialize Digital Pins
            _adcControl.OpenPin((int)ADCInPins.BusyPin, PinMode.Input);
            _adcControl.OpenPin((int)ADCOutPins.ResetPin, PinMode.Output);
            _adcControl.OpenPin((int)ADCOutPins.ConvertStartPin, PinMode.Output);
            _scale.FetchData();
            _scale.ScaleFactorsUpdate();
            
            
            

            // ADC Setup
            ADCReset();
            Task.Run(() => ADCLoop());
            Task.Run(() => AverageValuesSendDataPoint());

        }

        public Delegate[] AverageValuesDelegates(){
            return OnAverageValues.GetInvocationList();
        }


        public void InitSPI(){
            var settings = new SpiConnectionSettings(0,0){
                ClockFrequency = 25000000,
                Mode = SpiMode.Mode3,
            };

            spi = SpiDevice.Create(settings);

            Console.WriteLine(spi.ToString());
        }

        public void ADCLoop() {
            
            while(true){

                if(_adcControl.Read((int)ADCInPins.BusyPin) == PinValue.Low){

                    busyReading = true;

                    double[] ADCValues = Read();

                    for(int i = 0; i < readSums.Length; i ++){
                        readSums[i] += ADCValues[i];
                    }

                    busyReading = false;

                    while(busyAveraging){}
                    
                    _adcControl.Write((int)ADCOutPins.ConvertStartPin, PinValue.Low);
                    _adcControl.Write((int)ADCOutPins.ConvertStartPin, PinValue.High);
                }
            }
            
        }

        private void AverageValuesSendDataPoint(){
            while(true)
            {

                Thread.Sleep(250);

                while(busyReading) {};

                busyAveraging = true;

                if (SampleNum > 0)
                {

                    for (int i = 0; i < Avgs.Length; i++)
                    {
                        Avgs[i] = readSums[i] / SampleNum;
                        readSums[i] = 0;
                    }
                    SampleNum = 0;
                }
                else {
                    ADCReset();
                    
                    Console.WriteLine("Caught a zero sample point.");
                }

                busyAveraging = false;

                while(busyCalibrating) {}

                ScaledNums = ComputeScaledValues();
                
                
            }
        }
        
        private double[] ComputeScaledValues(){


            double[] ComputedArray = new double[ScaledNums.Length];

            for(int i = 0;  i < ScaledNums.Length; i++){
                if (i < 3){
                    // Mass, HPHR, and HPLR
                    ComputedArray[i] = (Avgs[i] - _scale.ZeroOffsets[i]) * _scale.ScaleFactors[i];
                }

                else if (i == (int)(ReadingTypes.Temp)){
                    // Analog to Kelvin
                    double AnalogTempResult = Avgs[i] / 6553.6;
                    if(AnalogTempResult != 0){
                        AnalogTempResult = 100000 * ((4.88 / AnalogTempResult) - 1);
                        AnalogTempResult = 1 / (0.000828083 + (0.000208691 * Math.Log(AnalogTempResult)) + 
                            (0.000000080812 * Math.Pow(Math.Log(AnalogTempResult), 3)));
                        
                        // Kelvin to Celsius
                        ComputedArray[i] = AnalogTempResult - 273.15;
                    }

                    else { ComputedArray[i] = ScaledNums[i]; }
                }

                else if (i == (int)(ReadingTypes.RH)){
                    ComputedArray[i] = (Avgs[i] - _scale.ZeroOffsets[i]) * _scale.ScaleFactors[i];
                }
            }
            return ComputedArray;
        }

        private double[] Read(){

            double[] returnVals = new double[5];
            string ConversionBuffer = "";
            byte[] bytebuf = new byte[16];
            spi.Read(bytebuf);
            // Iterate through bytes 1-10; each ADC Line has 2 bytes of information sent with it.

            for(int i = 0; i < 10; i++)
            {
                //0,2,4,6,8
                if(i%2 == 0){
                    // 16-bit first value
                    ConversionBuffer += Convert.ToString(bytebuf[i], 2).PadLeft(8, '0');
                }
                //1,3,5,7,9
                else{
                    ConversionBuffer += Convert.ToString(bytebuf[i], 2).PadLeft(8, '0');// Padded with another byte for 16-bit float value
                    returnVals[i/2] = Convert.ToInt32(ConversionBuffer, 2);
                    ConversionBuffer = "";
                }

                
            }
            // Check that none of our array values are NaN
            bool GoodValues = true;
            foreach(double val in returnVals){
                if(val == 0) GoodValues = false;
            }

            if(GoodValues)
            {
                SampleNum++;
                NaNCounter = 0;
            }
            else{
                ADCReset();
                for(int i = 0; i < returnVals.Length; i++)
                {
                    returnVals[i] = 0;
                }
                NaNCounter++;
            }

            return returnVals;
            
        }

        public void ADCReset(){
            _adcControl.Write((int)ADCOutPins.ResetPin, PinValue.High);
            Thread.Sleep(5);
            _adcControl.Write((int)ADCOutPins.ResetPin, PinValue.Low);
            
            _adcControl.Write((int)ADCOutPins.ConvertStartPin, PinValue.Low);
            _adcControl.Write((int)ADCOutPins.ConvertStartPin, PinValue.High);
        }
    }

    public class ADCPin {
        public bool isInput;

    }

}