using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.Devices.Spi;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Enumeration;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RasPiLCDSandbox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        SpiDevice screenSPI;

        public MainPage()
        {
            this.InitializeComponent();

            var selector = SpiDevice.GetDeviceSelector();
            
            var devicesTask = DeviceInformation.FindAllAsync(selector).AsTask();
            while (!devicesTask.IsCompleted) ;//devicesTask;
            var devices = devicesTask.Result;

            var screenSettings = new SpiConnectionSettings(1);
            screenSettings.ClockFrequency = 50000000;
            screenSettings.Mode = SpiMode.Mode0;
            screenSettings.DataBitLength = 8;
            screenSettings.SharingMode = SpiSharingMode.Shared;

            var screenSPITask = SpiDevice.FromIdAsync(devices[0].Id, screenSettings).AsTask();
            while (!screenSPITask.IsCompleted);
            screenSPI = screenSPITask.Result;

            lcd_reset();

            lcd_init();

            //lcd_fill(0x0000);
            lcd_fill2(0, 0, 480, 320, 0x0000);
            Delay(TimeSpan.FromMilliseconds(500));

            //lcd_fill(0xF800);
            lcd_fill2(0, 0, 100, 100, 0xF800);
            Delay(TimeSpan.FromMilliseconds(500));

            //lcd_fill(0x07E0);
            lcd_fill2(0, 0, 100, 100, 0x07E0);
            Delay(TimeSpan.FromMilliseconds(500));

            //lcd_fill(0x001F);
            lcd_fill2(0, 0, 100, 100, 0x001F);
            Delay(TimeSpan.FromMilliseconds(500));

            //lcd_fill(0xffff);
            lcd_fill2(0, 0, 100, 100, 0xffff);
            Delay(TimeSpan.FromMilliseconds(500));

            //lcd_fill(0x0000);
            lcd_fill2(0, 0, 100, 100, 0x0000);
        }

        private void Delay(TimeSpan timeSpan)
        {
            var waitTask = Task.Delay(timeSpan);
            while (!waitTask.IsCompleted) ;
        }

        private void lcd_reset()
        {
            screenSPI.Write(new byte[] {0, 0, 0, 0});
            Delay(TimeSpan.FromSeconds(1));
            screenSPI.Write(new byte[] {0, 0, 0, 2});
            Delay(TimeSpan.FromSeconds(1));
        }

        void lcd_init()
        {
            lcd_reset();
            Delay(TimeSpan.FromMilliseconds(100));
            lcd_cmd(0x0000);
            Delay(TimeSpan.FromMilliseconds(1));

            lcd_cmd(0x00B0);
            lcd_data(0x0000);
            lcd_cmd(0x0011);
            Delay(TimeSpan.FromMilliseconds(5)); //mdelay(50);

            lcd_cmd(0x00B3);
            lcd_data(0x0002);
            lcd_data(0x0000);
            lcd_data(0x0000);
            lcd_data(0x0000);

            lcd_cmd(0x0036);
            lcd_data(0x0028);

            lcd_cmd(0x003A);
            lcd_data(0x0055); //55 lgh

            lcd_cmd(0x00B6);
            lcd_data(0x0000);
            lcd_data(0x0002); //220 GS SS SM ISC[3:0]
            lcd_data(0x003B);

            lcd_cmd(0xE0);
            lcd_data(0x1f);
            lcd_data(0x2C);
            lcd_data(0x2C);
            lcd_data(0x0B);
            lcd_data(0x0C);
            lcd_data(0x04);
            lcd_data(0x4C);
            lcd_data(0x64);
            lcd_data(0x36);
            lcd_data(0x03);
            lcd_data(0x0E);
            lcd_data(0x01);
            lcd_data(0x10);
            lcd_data(0x01);
            lcd_data(0x00);

            lcd_cmd(0XE1);
            lcd_data(0x1f);
            lcd_data(0x3f);
            lcd_data(0x3f);
            lcd_data(0x0f);
            lcd_data(0x1f);
            lcd_data(0x0f);
            lcd_data(0x7f);
            lcd_data(0x32);
            lcd_data(0x36);
            lcd_data(0x04);
            lcd_data(0x0B);
            lcd_data(0x00);
            lcd_data(0x19);
            lcd_data(0x14);
            lcd_data(0x0F);

            lcd_cmd(0xE2);
            lcd_data(0x0f);
            lcd_data(0x0f);

            lcd_data(0x0f);

            lcd_cmd(0xE3);
            lcd_data(0x0f);
            lcd_data(0x0f);

            lcd_data(0x0f);

            lcd_cmd(0x13);

            lcd_cmd(0x0029);
            Delay(TimeSpan.FromMilliseconds(20));

            lcd_cmd(0x00B4);
            lcd_data(0x0000);
            Delay(TimeSpan.FromMilliseconds(20));


            lcd_cmd(0x002A);
            lcd_data(0x0000);
            lcd_data(0x0000);

            lcd_data(0x0001);
            lcd_data(0x000dF);

            lcd_cmd(0x002B);
            lcd_data(0x0000);
            lcd_data(0x0000);
            lcd_data(0x0001);
            lcd_data(0x003f);
        }

        void lcd_setptr()
        {
            lcd_cmd(0x002b);
            lcd_data(0x0000);
            lcd_data(0x0000); // 0
            lcd_data(0x0001);
            lcd_data(0x003f); //319

            lcd_cmd(0x002a);
            lcd_data(0x0000);
            lcd_data(0x0000); // 0
            lcd_data(0x0001);
            lcd_data(0x00df); // 479

            lcd_cmd(0x002c);
        }

        void lcd_setarea(ushort x, ushort y)
        {
            lcd_cmd(0x002b);
            lcd_data((ushort)((y >> 8)));
            lcd_data((ushort)((0x00ff & y)));
            lcd_data(0x0001);
            lcd_data(0x003f);

            lcd_cmd(0x002a);
            lcd_data((ushort)(x >> 8));
            lcd_data((ushort)(0x00ff & x));
            lcd_data(0x0001);
            lcd_data(0x00df);
            lcd_cmd(0x002c);
        }

        void lcd_setarea2(ushort sx, ushort sy, ushort x, ushort y)
        {

            if (sx > 479) sx = 0;
            if (sy > 319) sy = 0;
            if (x > 479) x = 479;
            if (y > 319) y = 319;

            lcd_cmd(0x002b);
            lcd_data((ushort)(sy >> 8));
            lcd_data((ushort)(0x00ff & sy));
            lcd_data((ushort)(y >> 8));
            lcd_data((ushort)(0x00ff & y));

            lcd_cmd(0x002a);
            lcd_data((ushort)(sx >> 8));
            lcd_data((ushort)(0x00ff & sx));
            lcd_data((ushort)(x >> 8));
            lcd_data((ushort)(0x00ff & x));

            lcd_cmd(0x002c);
        }

        void lcd_fill2(ushort sx, ushort sy, ushort x, ushort y, ushort color565)
        {
            ushort tmp = 0;
            int cnt;
            if (sx > 479) sx = 0;
            if (sy > 319) sy = 0;
            if (x > 479) x = 479;
            if (y > 319) y = 319;

            if (sx > x)
            {
                tmp = sx;
                sx = x;
                x = tmp;
            }

            if (sy > y)
            {
                tmp = sy;
                sy = y;
                y = tmp;
            }

            cnt = (y - sy) * (x - sx);
            lcd_setarea2(sx, sy, x, y);
            for (int t = 0; t < cnt; t++)
            {
                lcd_data(color565);
            }
        }

        void lcd_fill(ushort color565)
        {
            lcd_setptr();
            for (int x = 0; x < 153601; x++)
            {
                lcd_data(color565);
            }
        }

        private void lcd_data(ushort data)
        {
            lcd_transmit(data, 0x15);
            lcd_transmit(data, 0x1f);
        }

        private void lcd_cmd(ushort data)
        {
            lcd_transmit(data, 0x11);
            lcd_transmit(data, 0x1b);
        }

        private void lcd_transmit(ushort data, byte end)
        {
            byte first = (byte)(data >> 8);
            byte second = (byte)(data & 0x00ff);

            screenSPI.Write(new byte[] { 0, first, second, end});
        }

        private void lcd_transmit(ushort data, byte start, byte end)
        {
            byte first = (byte)(data >> 8);
            byte second = (byte)(data & 0x00ff);

            screenSPI.Write(new byte[] { 0, first, second, start, 0, first, second, end });
        }
    }
}
