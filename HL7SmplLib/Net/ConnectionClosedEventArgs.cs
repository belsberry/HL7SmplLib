using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7SmplLib.Net
{
    public class ConnectionClosedEventArgs : EventArgs
    {
        public string Reason { get; set; }
    }
}
