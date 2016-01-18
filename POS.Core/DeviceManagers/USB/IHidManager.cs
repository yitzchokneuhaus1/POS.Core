using HidLibrary;
using System.Threading.Tasks;
 
namespace POS.Core
{
    public interface IDevice
    {
        string DeviceName { get; }
        string DeviceId { get; }
        string ManufactureId { get; }
        bool IsConnected { get; }
        bool ConnectDevice();
        bool IsOpen { get; }
        void ReadDeviceData();
        event DeviceDataChangedEventArgs DataChanged;
        UnitOfMeasure UOM { get; }
        ScaleType ScaleType { get; }
        bool DisconectDevice();

    }

    public interface IHidDeviceWrapper : IDevice
    {
        IHidDevice HidDevice { get; }
    }


    public delegate void DeviceDataChangedEventArgs(object sender, DeviceDataEventArgs e);
}