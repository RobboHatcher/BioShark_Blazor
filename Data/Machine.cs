using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.IO;

namespace BioShark_Blazor.Data {

    public class Machine {

        // EDIT THIS TO CHANGE PIN NUMS
        public enum OutputPins {
            Cat = 5, Heat = 6, Blower = 12, LRCat = 13,
            Mist = 16, RunPump = 17, FillPump = 18, MistFan = 19, Distribution = 20,
            HRCat = 21, Sidekick = 26 , Drainpump = 27,
        }

        int inputMisterLevel = 4; // Mister level sensor: active low

        public int TestPin = 24;
        public int VerifyNum = 0;

        private GpioController _controller { get; set; }
        private List<Sensor> _sensors;
        //private ProcessController _controller;
        //private ADC _adc;

        public Machine () {


            Random rand = new Random (DateTime.Now.Millisecond);
            VerifyNum = rand.Next (0, 100);
            Console.WriteLine (VerifyNum);
            _controller = new GpioController ();
            //OUTPUTS

            //bool = is the sensor Active high?
            _sensors = new List<Sensor> ();
            _sensors.Add (new Sensor ((int) OutputPins.Cat, true));
            _sensors.Add (new Sensor ((int) OutputPins.Heat, true));
            _sensors.Add (new Sensor ((int) OutputPins.Blower, false));
            _sensors.Add (new Sensor ((int) OutputPins.LRCat, false));
            _sensors.Add (new Sensor ((int) OutputPins.Mist, false));
            _sensors.Add (new Sensor ((int) OutputPins.RunPump, true));
            _sensors.Add (new Sensor ((int) OutputPins.FillPump, true));
            _sensors.Add (new Sensor ((int) OutputPins.MistFan, false));
            _sensors.Add (new Sensor ((int) OutputPins.Distribution, false));
            _sensors.Add (new Sensor ((int) OutputPins.HRCat, false));
            _sensors.Add (new Sensor ((int) OutputPins.Sidekick, false));
            _sensors.Add (new Sensor ((int) OutputPins.Drainpump, false));

            foreach (var sensor in _sensors) {
                OpenOutPinToOffState (sensor.PinNum);
            }

            try {
                _controller.OpenPin (inputMisterLevel, PinMode.Input);
            } catch {
                Console.WriteLine ("Pin already open, setting to new pin Mode");
                _controller.SetPinMode (inputMisterLevel, PinMode.Input);
            }
        }

        // By default, OpenPin will open out.
        private void OpenOutPinToOffState (int sensorPin) {
            if (!_controller.IsPinOpen (sensorPin)) {
                _controller.OpenPin (sensorPin, PinMode.Output);
                TurnOff (sensorPin);
            }
        }

        public void TurnOn (int sensorPin) {
            if (sensorPin == inputMisterLevel)
                return;
            if (_sensors[GetSensorPinIndex (sensorPin)].IsActiveHigh) {
                _controller.Write (sensorPin, PinValue.High);
            } else {
                _controller.Write (sensorPin, PinValue.Low);
            }
            _sensors[GetSensorPinIndex (sensorPin)].TurnOn ();

        }

        public void TurnOff (int sensorPin) {
            if (sensorPin == inputMisterLevel)
                return;
            if (_sensors[GetSensorPinIndex (sensorPin)].IsActiveHigh ) {
                _controller.Write (sensorPin, PinValue.Low);
            } else {
                _controller.Write (sensorPin, PinValue.High);
            }
            _sensors[GetSensorPinIndex (sensorPin)].TurnOff ();
        }

        public bool IsOn (int sensorPin) {
            return _sensors[GetSensorPinIndex (sensorPin)].IsOn ();
        }

        private int GetSensorPinIndex (int PinNum) {
            return _sensors.IndexOf (_sensors.Find (pin => pin.PinNum == PinNum));
        }

    }

    public class Sensor {
        private bool On = false;
        public bool IsActiveHigh { get; set; } = false;
        public int PinNum { get; set; }
        public Sensor (int _pin, bool _isActiveHigh) {
            PinNum = _pin;
            IsActiveHigh = _isActiveHigh;
        }

        public void TurnOn () {
            On = true;
        }

        public void TurnOff () {
            On = false;
        }

        public bool IsOn () {
            return On;
        }

    }

}