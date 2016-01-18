using System;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace POS.Core.DeviceManagers.Serial
{
    public class SerialDeviceWrapper : ISerialDevice
    {
        public string DeviceName { get; private set; }
        public string DeviceId { get; private set; }
        public string ManufactureId { get; private set; }
        private bool _isReadingInProcess = false;

        public SerialDeviceWrapper(string portName)
        {
            DeviceName = portName;

        }

        public bool IsReadInProcess
        {
            get { return _isReadingInProcess; }
        }

        public ScaleType ScaleType
        {
            get { return ScaleType.Serial; }
        }

        public bool DisconectDevice()
        {
            if (IsConnected)
                SerialDevice.Close();
            Console.WriteLine("Device disconnected..");
            return IsConnected == false;
        }


        public bool IsConnected
        {
            get { return SerialDevice.IsOpen; }

        }
        public bool ConnectDevice()
        {
            SerialDevice = SerialDevice ?? new SerialPort(DeviceName);
            if (SerialPort.GetPortNames().Any(e => e == DeviceName) == false)
                return false;
            if (IsOpen == false)
            {
                SerialDevice.Open();
            }

            Thread.Sleep(500);
            SerialDevice.DataReceived += SerialDevice_DataReceived;
            return IsConnected;
        }

        void SerialDevice_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_isReadingInProcess)
                    return;
                _isReadingInProcess = true;
                var serialPortdevice = (SerialPort)sender;
                if (serialPortdevice == null)
                {
                    return;
                }

                while (serialPortdevice.BytesToRead > 0)
                {
                    int byte_count = serialPortdevice.BytesToRead;
                    byte[] buffer = new byte[byte_count];

                    int read_count = serialPortdevice.Read(buffer, 0, byte_count);
                    var originialBuffer = buffer.ToList().ToArray();
                    var ignoredValues = new[] { 10, 3, 160, 141, 226 };

                    buffer = buffer.Where(r => ignoredValues.Contains(r) == false).Select(CleanArrayValue).ToArray();

                    if (buffer.Length == 0)
                        return;
                    if (buffer[0] == 178)
                        buffer[0] = 50;

                    // PROCESS DATA HERE
                    string echo = Encoding.ASCII.GetString(buffer, 0, buffer.Length);

                    var date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                    var cleanString = (Regex.Replace(echo, "[^0-9A-Za-z]+", "")).Replace("\n", "").Replace(" ", "").Replace("\r", "");

                    var output = string.Format("Thread Id  {3} -- New read at {0} - Data {1} Original Data {2}", date, cleanString, echo, Thread.CurrentThread.ManagedThreadId);

                    //   Console.WriteLine(output);

                    var stringLines = echo.Split('l');
                    if (stringLines.Length <= 0)
                        return;

                    var firstLine = stringLines[0];
                    if (string.Equals(firstLine, "?", StringComparison.CurrentCultureIgnoreCase) || string.Equals(firstLine, ".", StringComparison.CurrentCultureIgnoreCase) || string.Equals(firstLine, "0", StringComparison.CurrentCultureIgnoreCase) || string.Equals(firstLine, "0.", StringComparison.CurrentCultureIgnoreCase))
                        return;

                    firstLine = firstLine.Replace("l", "").Replace("L", "").Replace("?", "");
                    if (string.IsNullOrEmpty(firstLine))
                        return;

                    if (stringLines.Length == 6)
                        firstLine = String.Concat(stringLines.Where(line =>
                            string.IsNullOrEmpty(line) == false && line != "00")).Replace("l", "");


                    var data = new DeviceDataEventArgs(firstLine, Encoding.ASCII.GetString(originialBuffer, 0, originialBuffer.Length));
                    OnDeviceDataChanged(data);
                    Console.WriteLine("Lbs: {0} Ozs {1}", data.Lbs, data.Ozs);
                    return;

                }


            }
            catch
            {
                Console.WriteLine("Error happened");

            }
            finally
            {
                Thread.Sleep(250);
                _isReadingInProcess = false;

            }
        }

        private byte CleanArrayValue(byte val)
        {
            if (val == 177)
                return 49;
            if (val == 178)
                return 50;
            if (val == 179)
                return 51;
            if (val == 180)
                return 52;
            if (val == 181)
                return 53;
            if (val == 182)
                return 54;
            if (val == 183)
                return 55;
            if (val == 184)
                return 56;
            if (val == 185)
                return 57;


            return val;
        }

        public bool ConnectDevice(string portName)
        {
            SerialDevice = new SerialPort(portName);
            return ConnectDevice();
        }

        public bool IsOpen
        {
            get { return SerialDevice.IsOpen; }

        }

        private static object lockObject = new object();

        public void ReadDeviceData()
        {
            if (IsConnected == false)
                throw new Exception("Cannot find connected device.");
            lock (lockObject)
            {
                SerialDevice.WriteTimeout = 500;
                SerialDevice.Write("W\r");
                Thread.Sleep(250);
            }

        }

        public event DeviceDataChangedEventArgs DataChanged;
        public UnitOfMeasure UOM { get; private set; }
        public SerialPort SerialDevice { get; set; }
        private DeviceDataEventArgs lastDataRead;

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
    }
}
