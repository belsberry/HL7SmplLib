using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7SmplLib
{
    public class HL7MLLP
    {
        /// <summary>
        /// Starting byte in the MLLP wrapper
        /// </summary>
        public const byte VT = 0x0B;
        /// <summary>
        /// Ending byte in the MLLP wrapper
        /// </summary>
        public const byte FS = 0x1C;
        /// <summary>
        /// Carriage Return
        /// </summary>
        public const byte CR = 0x0D;

        public const char MESSAGE_BEGIN = (char)VT;
        public const char MESSAGE_END = (char)FS;

        public static string WrapMessage(string message)
        {
            return (char)VT + message + (char)FS + (char)CR;
        }
        public static string Trim(string message)
        {
            return message.Trim((char)VT, (char)FS, (char)CR);
        }
    }
}
