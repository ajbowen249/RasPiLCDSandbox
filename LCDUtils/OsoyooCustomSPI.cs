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
            _mosi = 10;
            _miso = 9;
            _clk = 11;
            _cs = 7;

            _clockSpeed = 50000000;
            _interval = 1.0 / (double)_clockSpeed;
        }

        public void OpenPins()
        {
            var gpioController = GpioController.GetDefault();

            _mosiPin = gpioController.OpenPin(_mosi);
            _misoPin = gpioController.OpenPin(_miso);
            _clkPin = gpioController.OpenPin(_clk);
            _csPin = gpioController.OpenPin(_cs);
        }
    }
}
