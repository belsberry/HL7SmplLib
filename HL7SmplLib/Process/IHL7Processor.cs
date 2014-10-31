using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7SmplLib.Process
{
    public interface IHL7Processor
    {
        HL7Message ProcessHL7(HL7Message message);
    }
}
