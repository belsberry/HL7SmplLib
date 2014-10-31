using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7SmplLib
{
    public class HL7CharSet
    {
        private string segmentSeparator = "\r";
        private char fieldSeparator;
        private char componentSeparator;
        private char subcomponentSeparator;
        private char repeatSeparator;

        public string SegmentSeparator
        {
            get
            {
                return segmentSeparator;
            }
            private set
            {
                segmentSeparator = value;
            }
        }

        public char FieldSeparator { get { return fieldSeparator; } }
        public char ComponentSeparator { get { return componentSeparator; } }
        public char SubComponentSeparator { get { return subcomponentSeparator; } }
        public char RepeatChar { get { return repeatSeparator; } }


        public HL7CharSet() : this('|', '^', '&', '~') { }
        public HL7CharSet(char field, char component, char subcomponent, char repeat)
        {
            fieldSeparator = field;
            componentSeparator = component;
            subcomponentSeparator = subcomponent;
            repeatSeparator = repeat;
        }

        public void SetSegmentSeparator(LineEnding lineEnding)
        {
            if (lineEnding == LineEnding.CarriageReturn)
            {
                SegmentSeparator = "\r";
            }
            else if (lineEnding == LineEnding.CarriageReturnLineFeed)
            {
                SegmentSeparator = "\r\n";
            }
            else if (lineEnding == LineEnding.LineFeed)
            {
                SegmentSeparator = "\n";
            }
        }
    }
}
