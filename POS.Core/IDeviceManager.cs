using System.Collections.Generic;

namespace POS.Core
{
    public interface IDeviceManager
    {
        bool Connect(IDevice device);
        List<IDevice> GetConnectedDevices();
        void StartReading(IDevice device = null, int threads = 0);
        IDevice ConnectedDevice { get; }
        ScaleType ScaleType { get; }
        bool StopDevice(IDevice deivce);
        bool IsReading { get; }
    }
}