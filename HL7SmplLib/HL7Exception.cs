using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7SmplLib
{
    public class HL7Exception : Exception
    {
        public HL7Exception(string message)
            : base(message) { }
    }
}
