using System;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;

namespace POS.Core
{
    public class HidDeviceWrapper : IHidDeviceWrapper
    {
        public HidDeviceWrapper(IHidDevice initDevice)
        {
            _hidDevice = initDevice;

        }

        public ScaleType ScaleType
        {
            get { return ScaleType.USB; }
        }

        public bool DisconectDevice()
        {
            _hidDevice.CloseDevice();
            return _hidDevice.IsConnected == false;
        }

        public string DeviceId
        {
            get { return _hidDevice.Attributes.ProductId.ToString(); }
        }

        public string ManufactureId
        {
            get { return _hidDevice.Attributes.VendorId.ToString(); }
        }

        private UnitOfMeasure _deviceUnitOfMeasure;
        private IHidDevice _hidDevice;

        public IHidDevice HidDevice
        {
            get { return _hidDevice; }
        }

        public bool IsConnected
        {
            get { return _hidDevice.IsConnected; }
        }

        public bool ConnectDevice()
        {
            var connectionTries = 0;
            if (_hidDevice == null)
                throw new Exception("No device found.");

            while (connectionTries < 10 && (_hidDevice.IsConnected == false || _hidDevice.IsOpen == false))
            {
                Thread.Sleep(50);
                connectionTries++;
                _hidDevice.OpenDevice(DeviceMode.NonOverlapped, DeviceMode.NonOverlapped, ShareMode.ShareRead);
            }

            if (connectionTries >= 10 && _hidDevice.IsConnected == false)
                throw new Exception("Could not connect to device.");

            return IsConnected;
        }

        public bool IsOpen
        {
            get { return _hidDevice.IsOpen; }
        }

        public void ReadDeviceData()
        {
            if (!IsConnected)
                throw new Exception("USB Device not connected.");

            var dataResponse = _hidDevice.Read();

            _deviceUnitOfMeasure = (UnitOfMeasure)dataResponse.Data[3];
            OnDeviceDataChanged(new DeviceDataEventArgs(dataResponse));
        }

        public event DeviceDataChangedEventArgs DataChanged;

        public string DeviceName
        {
            get { return _hidDevice.Description; }
        }

        private DeviceDataEventArgs lastDataRead = null;

        protected virtual void OnDeviceDataChanged(DeviceDataEventArgs data)
        {
            var handler = DataChanged;


            if (handler != null && ScaleHasChanged(data))
                handler(this, data);

            lastDataRead = (DeviceDataEventArgs)data.Clone();
        }

        private bool ScaleHasChanged(DeviceDataEventArgs newData)
        {
            if (lastDataRead == null)
                return true;

            if (lastDataRead.Lbs != newData.Lbs)
                return true;

            if (lastDataRead.Ozs != newData.Ozs)
                return true;

            return false;
        }

        public UnitOfMeasure UOM
        {
            get { return _deviceUnitOfMeasure; }
        }
    }


}
