using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace POS.Core.DeviceManagers.Serial
{
    public class SerialDeviceManager : IDeviceManager
    {

        private ISerialDevice _connectedDevice;
        private bool readingFromDevice = false;
        private int? threads = 0;
        public bool Connect(IDevice device)
        {
            device.ConnectDevice();
            if (device.IsConnected)
                _connectedDevice = (ISerialDevice)device;

            readingFromDevice = true;
            return device.IsConnected;
        }

        public List<IDevice> GetConnectedDevices()
        {
            var devices = SerialPort.GetPortNames();
            return devices.Select(device => new SerialDeviceWrapper(device)).Cast<IDevice>().ToList();

        }

        private System.Timers.Timer timer;
        public void StartReading(IDevice device = null, int threads = 0)
        {
            _connectedDevice = _connectedDevice ?? (ISerialDevice)device;
            this.threads = threads == 0 ? (int?)null : threads;
            timer = timer ?? new System.Timers.Timer(250);

            timer.Enabled = true;
            timer.Elapsed += (sender, args) =>
            {
                timer.Enabled = false;
                InternalReader();
                timer.Enabled = true;
            };

            timer.Start();

        }


        private void InternalReader()
        {
            try
            {
                if (_connectedDevice.IsReadInProcess)
                    return;

                Read(_connectedDevice);
            }
            catch
            {
            }
        }

        private void Read(ISerialDevice serialDevice)
        {
            if (!readingFromDevice)
            {
                return;
            }

            if (serialDevice.IsConnected == false || serialDevice.IsOpen == false)
                Connect(serialDevice);
            if (serialDevice.IsReadInProcess)
            {
                Thread.Sleep(250);
                return;
            }
            serialDevice.ReadDeviceData();
        }
         
        public IDevice ConnectedDevice
        {
            get { return _connectedDevice; }
        }

        public ScaleType ScaleType
        {
            get { return ScaleType.Serial; }
        }

        public bool StopDevice(IDevice deivce)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Enabled = false;
            }

            readingFromDevice = false;
            return deivce.DisconectDevice();
        }

        public bool IsReading
        {
            get { return readingFromDevice; }
        }
    }
}