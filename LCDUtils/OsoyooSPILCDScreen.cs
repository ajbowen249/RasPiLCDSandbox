using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;

namespace LCDUtils
{
    public class OsoyooSPILCDScreen
    {
        const byte Control_CMDStart = 0x11;
        const byte Control_CMDEnd = 0x1b;
        const byte Control_DataStart = 0x15;
        const byte Control_DataEnd = 0x1f;

        int _spiChannel;
        int _clockSpeed;
        SpiDevice _screenSPI;

        public OsoyooSPILCDScreen(int spiChannel, int clockSpeed)
        {
            _spiChannel = spiChannel;
            _clockSpeed = clockSpeed;
        }

        public async Task InitializeSPI_Async()
        {
            var selector = SpiDevice.GetDeviceSelector();

            var devices = await DeviceInformation.FindAllAsync(selector);

            var screenSettings = new SpiConnectionSettings(_spiChannel);
            screenSettings.ClockFrequency = _clockSpeed;
            screenSettings.Mode = SpiMode.Mode0;
            screenSettings.DataBitLength = 8;
            screenSettings.SharingMode = SpiSharingMode.Shared;

            _screenSPI = await SpiDevice.FromIdAsync(devices[0].Id, screenSettings);
        }

        public void InitializeLCD()
        {
            ResetLCD();

            SyncDelayMS(100);
            SendCommand(ILI9341Constants.NoOp);
            SyncDelayMS(1);

            SendCommand(ILI9341Constants.RGBInterfaceSignalControl, 0x00);

            SendCommand(ILI9341Constants.SleepModeOut);
            SyncDelayMS(5);

            SendCommand(ILI9341Constants.FrameControlPartialMode, 0x02, 0x00, 0x00, 0x00);
            SendCommand(ILI9341Constants.MemoryAccessControl, 0x28);
            SendCommand(ILI9341Constants.PixelFormatSet, 0x55);
            SendCommand(ILI9341Constants.DisplayFunctionControl, 0x00, 0x02, 0x3B);

            SendCommand(ILI9341Constants.PositiveGammaCorrection, 0x1f, 0x2C, 0x2C, 0x0B, 0x0C, 0x04, 0x4C, 0x64, 0x36, 0x03, 0x0E, 0x01, 0x10, 0x01, 0x00);
            SendCommand(ILI9341Constants.NegativeGammaCorrection, 0x1f, 0x3f, 0x3f, 0x0f, 0x1f, 0x0f, 0x7f, 0x32, 0x36, 0x04, 0x0B, 0x00, 0x19, 0x14, 0x0F);
            SendCommand(ILI9341Constants.DigitalGammaControl, 0x0f, 0x0f, 0x0f);
            SendCommand(ILI9341Constants.DigitalGammaControl2, 0x0f, 0x0f, 0x0f);

            SendCommand(ILI9341Constants.NormalDisplayOn);
            SendCommand(ILI9341Constants.DisplayOn);
            SyncDelayMS(20);

            SendCommand(ILI9341Constants.DisplayInversionControl, 0x00);
            SyncDelayMS(20);

            SendCommand(ILI9341Constants.ColumnAddressSet, 0x00, 0x00, 0x01, 0xdf);
            SendCommand(ILI9341Constants.PageAddressSet, 0x00, 0x00, 0x01, 0x3f);
        }

        public void ResetLCD()
        {
            _screenSPI.Write(new byte[] { 0, 0, 0, 0 });
            SyncDelayMS(1000);
            _screenSPI.Write(new byte[] { 0, 0, 0, 2 });
            SyncDelayMS(1000);
        }

        private void SendCommand(byte command)
        {
            SendCommandOrData(command, Control_CMDStart);
            SendCommandOrData(command, Control_CMDEnd);
        }

        private void SendCommand(byte command, params byte[] args)
        {
            SendCommand(command);
            foreach (var arg in args) SendData(arg);
        }

        private void SendData(byte data)
        {
            SendCommandOrData(data, Control_DataStart);
            SendCommandOrData(data, Control_DataEnd);
        }

        private void SendCommandOrData(byte item, byte control)
        {
            _screenSPI.Write(new byte[] { 0, 0, item, control});
        }

        private void SyncDelayMS(int delayTime)
        {
            var waitTask = Task.Delay(TimeSpan.FromMilliseconds(delayTime));
            while (!waitTask.IsCompleted) ;
        }
    }
}
