using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace LCDUtils
{
    public class OsoyooCustomSPI
    {
        int _mosi;
        int _miso;
        int _clk;
        int _cs;

        GpioPin _mosiPin;
        GpioPin _misoPin;
        GpioPin _clkPin;
        GpioPin _csPin;

        int _clockSpeed;
        double _interval;

        public OsoyooCustomSPI()
        {
            _mosi = 5;
            _miso = 6;
            _clk = 13;
            _cs = 12;

            _clockSpeed = 50000000;
            _interval = 1.0 / (double)_clockSpeed;
        }

        public void Initialize()
        {
            var gpioController = GpioController.GetDefault();
            
            _mosiPin = gpioController.OpenPin(_mosi);
            _misoPin = gpioController.OpenPin(_miso);
            _clkPin = gpioController.OpenPin(_clk);
            _csPin = gpioController.OpenPin(_cs);

            _mosiPin.Write(GpioPinValue.Low);
            _mosiPin.Write(GpioPinValue.Low);
            _clkPin.Write(GpioPinValue.Low);
            _csPin.Write(GpioPinValue.High);
        }

        public void Write(byte[] data)
        {
            var bits = new GpioPinValue[data.Length * 8];

            for (int i = 0; i < data.Length; i++)
            {
                var datumValues = GetBits(data[i]);
                datumValues.CopyTo(bits, i * 8);
            }

            _csPin.Write(GpioPinValue.Low);
            SyncWaitInterval();

            foreach(var bit in bits)
            {
                _mosiPin.Write(bit);
                _clkPin.Write(GpioPinValue.High);
                SyncWaitInterval();
                _clkPin.Write(GpioPinValue.Low);
                SyncWaitInterval();
            }

            _csPin.Write(GpioPinValue.High);
            SyncWaitInterval();
        }

        private GpioPinValue[] GetBits(byte data)
        {
            var array = new GpioPinValue[8];
            byte mask = 0x01;
            for (int i = 0; i < 8; i++)
            {
                array[i] = (data & mask) > 0 ? GpioPinValue.High : GpioPinValue.Low;
                mask <<= 1;
            }

            return array;
        }

        private void SyncWaitInterval()
        {
            var waitTask = Task.Delay(TimeSpan.FromSeconds(_interval));
            //var waitTask = Task.Delay(TimeSpan.FromMilliseconds(1));
            while (!waitTask.IsCompleted) ;
        }
    }
}
