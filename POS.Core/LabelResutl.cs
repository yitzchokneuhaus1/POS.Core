using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POS.Core.Enums;

namespace POS.Core
{
    public class LabelResult
    {
        public string ZippedData { get; set; }
        public string PrinterName { get; set; }
        public int NumberOfCopies { get; set; }
        public PrintingType PrintingType { get; set; }
    }
}
