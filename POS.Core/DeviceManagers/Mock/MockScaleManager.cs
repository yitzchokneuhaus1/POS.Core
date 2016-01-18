using System.Collections.Generic;

namespace POS.Core.DeviceManagers.Mock
{
    public class MockScaleManager : IDeviceManager
    {
        private bool isRunning = true;
        private IDevice _connectedDevice = null;
        public bool Connect(IDevice device)
        {
            isRunning = true;
            _connectedDevice = device;
            return isRunning;
        }

        public List<IDevice> GetConnectedDevices()
        {
            return new List<IDevice>() { _connectedDevice ?? new MockDevice() };
        }


        private System.Timers.Timer timer;
        public void StartReading(IDevice device = null, int threads = 0)
        {
            timer = timer ?? new System.Timers.Timer(60000);

            timer.Enabled = true;
            timer.Elapsed += (sender, args) =>
            {
                timer.Enabled = false;

                _connectedDevice.ReadDeviceData();

                timer.Enabled = true;
            };

            timer.Start();

        }

        public IDevice ConnectedDevice
        {
            get { return _connectedDevice; }
        }

        public ScaleType ScaleType
        {
            get { return ScaleType.Mock; }
        }

        public bool StopDevice(IDevice deivce)
        {
            isRunning = false;
            return isRunning;
        }

        public bool IsReading
        {
            get { return isRunning; }
        }
    }
}
