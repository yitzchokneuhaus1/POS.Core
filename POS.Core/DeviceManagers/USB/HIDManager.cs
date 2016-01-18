using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;

namespace POS.Core
{
    public class HidManager : IDeviceManager
    {
        private IHidDeviceWrapper _connectedDevice;
        private bool readingFromDevice = false;
        private List<string> supportedManufactirresList = new List<string>() { "9332", "3768", "2338" };
        public bool Connect(IDevice device)
        {
            device.ConnectDevice();
            if (device.IsConnected)
                _connectedDevice = (IHidDeviceWrapper)device;

            return device.IsConnected;
        }

        public List<IDevice> GetConnectedDevices()
        {
            return HidLibrary.HidDevices.Enumerate().
                ToList().Select(ConvertDevice).Cast<IDevice>().Where(e => supportedManufactirresList.Contains(e.ManufactureId)).ToList();
        }

        public void StartReading(IDevice device = null, int threads = 0)
        {
            readingFromDevice = true;
            device = device ?? _connectedDevice;

            while (((threads == 0) || (threads > 0 && threads != 1)) || readingFromDevice == false)
            {
                if (threads > 0)
                    threads--;
                Read((IHidDeviceWrapper)device);
            }
            readingFromDevice = false;
        }

        private void Read(IHidDeviceWrapper hidDevice)
        {
            if (hidDevice.IsConnected == false || hidDevice.IsOpen == false)
                Connect(hidDevice);

            hidDevice.ReadDeviceData();
        }

        private HidDeviceWrapper ConvertDevice(HidDevice initDevice)
        {
            return new HidDeviceWrapper(initDevice);
        }


        public IDevice ConnectedDevice
        {
            get { return _connectedDevice; }
        }

        public ScaleType ScaleType
        {
            get { return ScaleType.USB; }
        }

        public bool StopDevice(IDevice deivce)
        {
            readingFromDevice = false;
            return deivce.DisconectDevice();
        }

        public bool IsReading
        {
            get { return readingFromDevice; }
        }
    }
}
