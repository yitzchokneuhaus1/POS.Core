using System;
using HidLibrary;

namespace POS.Core
{
    public class DeviceDataEventArgs : EventArgs, ICloneable
    {
        public string DebugDataString { get; private set; }
        public byte[] RawData { get; private set; }
        public int Lbs { get; private set; }
        public decimal Ozs { get; private set; }
        public decimal TotalWeightInOzs { get; private set; }
        public decimal WeightInFraction { get; set; }

        private HidDeviceData _deviceData;

        public DeviceDataEventArgs(HidDeviceData responseData)
        {
            if (responseData.Status != HidDeviceData.ReadStatus.Success)
                return;
            RawData = responseData.Data;
            ParseResponseData(responseData.Data);
            _deviceData = responseData;
        }

        public DeviceDataEventArgs(string weight, string debugData)
        {
            if (string.IsNullOrEmpty(debugData) == false)
            {
                Console.WriteLine("Debug data -- " + debugData);
            }

            DebugDataString = debugData;
            ParseRawStringData(weight);
        }

        private void ParseRawStringData(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }
            if (data.Contains(".") == false)
            {
                data = data.Insert(0, "0.");
            }
            if (string.IsNullOrEmpty(data.Split('.')[1]))
            {
                var indexOfPeriod = data.IndexOf('.');
                data = data.Insert(indexOfPeriod + 1, "0");

            }

            var weight = decimal.Parse(data);

            Lbs = (int)weight % 16;
            Ozs = (decimal.Parse("0." + weight.ToString().Split('.')[1]) * 16.00m);
            TotalWeightInOzs = (decimal)(Lbs * 16.00) + Ozs;
            WeightInFraction = TotalWeightInOzs / 16;


        }

        private void ParseResponseData(byte[] data)
        {
            // Byte 0 == Report ID?
            // Byte 1 == Scale Status (1 == Fault, 2 == Stable @ 0, 
            //      3 == In Motion, 4 == Stable,
            //      5 == Under 0, 6 == Over Weight, 
            //      7 == Requires Calibration, 8 == Requires Re-Zeroing)

            // Byte 2 == Weight Unit
            // Byte 3 == Data Scaling (decimal placement)
            // Byte 4 == Weight LSB (Least significant bit) 
            // Byte 5 == Weight MSB (Most significant bit)

            // FIXME: dividing by 100 probably wont work with
            // every scale, need to figure out what to do with
            // Byte 3

            var deviceWeightingUnit = (UnitOfMeasure)data[2];

            int num = data[5];
            var dividePercision = (data[3] == 254 ? 100.00M : 10.00M);//we need to determine what the division number is, either by 100 basses or base 10
            var lbs = data[4];

            DebugDataString = string.Format("Scale weight type: {0}, Scale Percssion: {1}, Byte 4: {2}, Byte 5: {3}",
                deviceWeightingUnit.ToString(), dividePercision, lbs, num);

            switch (deviceWeightingUnit)
            {
                case UnitOfMeasure.Ounces:

                    decimal totalOzsOnScale = (lbs + (num * 256)) / dividePercision;

                    Lbs = (int)totalOzsOnScale / 16;
                    Ozs = totalOzsOnScale % 16.0M;
                    TotalWeightInOzs = totalOzsOnScale;

                    break;
                case UnitOfMeasure.Pounds:

                    Lbs = (int)((lbs + (num * 256)) / dividePercision);
                    Ozs = 16 * (((lbs + (num * 256)) / dividePercision) % 1.0M);

                    TotalWeightInOzs = (Ozs + (Lbs * 16.0M));
                    break;
            }
            WeightInFraction = TotalWeightInOzs / 16;
        }


        public object Clone()
        {
            return new DeviceDataEventArgs(_deviceData);
        }
    }
}