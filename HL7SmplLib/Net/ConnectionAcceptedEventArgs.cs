using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7SmplLib.Net
{
    public class ConnectionAcceptedEventArgs : EventArgs
    {
        //public TcpClient Client {get;set;}
        public string Address { get; set; }
        public int Port { get; set; }

    }
}
