using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7SmplLib.Net
{
    public class HL7MessageReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string HeaderLine { get; set; }
        public string SourceAddress { get; set; }
        public int SourcePort { get; set; }
    }
}
