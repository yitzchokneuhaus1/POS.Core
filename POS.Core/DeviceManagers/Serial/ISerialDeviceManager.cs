using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Core
{
    public interface ISerialDevice : IDevice
    {
        SerialPort SerialDevice { get; set; }
        bool ConnectDevice(string portNmae);
        bool IsReadInProcess { get; }
    }


}
