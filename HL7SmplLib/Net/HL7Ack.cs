using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7SmplLib.Net
{
    public class HL7Ack
    {
        private HL7AckCode ackCode = HL7AckCode.Accept;

        public HL7AckCode AckCode
        {
            get { return ackCode; }
            set { ackCode = value; }
        }

        private string errorAckMessage;
        public string ErrorAckMessage
        {
            get { return errorAckMessage; }
            set { errorAckMessage = value; }
        }       

        private string customAckHL7Message;
        public string CustomAckHL7Message
        {
            get { return customAckHL7Message; }
            set { customAckHL7Message = value; }
        }        
    }
}
