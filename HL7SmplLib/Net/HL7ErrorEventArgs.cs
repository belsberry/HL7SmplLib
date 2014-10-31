using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7SmplLib.Net
{
    public class HL7ErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; set; }
        public string Ack { get; set; }
        public string HL7Message { get; set; }
    }
}
