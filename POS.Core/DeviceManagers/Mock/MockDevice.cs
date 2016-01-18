using System;

namespace POS.Core.DeviceManagers.Mock
{
    public class MockDevice : IDevice
    {

        public string DeviceName
        {
            get { return "Mock Scale"; }
        }

        public string DeviceId
        {
            get { return "0"; }
        }

        public string ManufactureId
        {
            get { return "0"; }
        }

        public bool IsConnected
        {
            get { return true; }
        }

        public bool ConnectDevice()
        {
            throw new NotImplementedException();
        }

        public bool IsOpen
        {
            get { throw new NotImplementedException(); }
        }

        public void ReadDeviceData()
        {
            OnDeviceDataChanged(new DeviceDataEventArgs("0.85", string.Empty));
        }

        public event DeviceDataChangedEventArgs DataChanged;

        public UnitOfMeasure UOM
        {
            get { return UnitOfMeasure.Ounces; }
        }

        public ScaleType ScaleType
        {
            get { return ScaleType.Mock; }
        }

        public bool DisconectDevice()
        {
            throw new NotImplementedException();
        }
        private bool ScaleHasChanged(DeviceDataEventArgs newData)
        {
            if (lastDataRead == null)
                return true;

            if (lastDataRead.Lbs != newData.Lbs)
                return true;

            return lastDataRead.Ozs != newData.Ozs;
        }

        protected virtual void OnDeviceDataChanged(DeviceDataEventArgs data)
        {
            var handler = DataChanged;

            if (handler == null)
                return;
            if (ScaleHasChanged(data) == false)
            {
                return;
            }

            handler(this, data);
            lastDataRead = (DeviceDataEventArgs)data;
        }

        private DeviceDataEventArgs lastDataRead;
    }
}