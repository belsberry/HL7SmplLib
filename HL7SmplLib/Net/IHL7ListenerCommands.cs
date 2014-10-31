using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7SmplLib.Net
{
    public interface IHL7ListenerCommands
    {
        HL7Ack ProcessMessage(string hl7Message);
    }
}
