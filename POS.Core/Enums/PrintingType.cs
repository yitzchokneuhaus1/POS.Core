using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Core.Enums
{
    public enum PrintingType
    {
        [Description("Printer Programming Language")]
        PPL = 1,
        [Description("Image")]
        Image = 2
    }
}
