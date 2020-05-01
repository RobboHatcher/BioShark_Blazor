using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;
using System.IO;

namespace BioShark_Blazor.Data {

    
    public class ADC {

        public enum ADCOutPins { ResetPin = 22, ConvertStartPin = 24 }
        public enum ADCInPins { BusyPin = 25 }
        GpioController _analogIn;

        public List<ADCPin> _inputs;
        public List<ADCPin> _outputs;  

        public ADC() {
            _analogIn = new GpioController();
            
        }

        public void InitSPI(){

        }
    }

    public class ADCPin {
        
    }

}