using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7SmplLib
{
    public class HL7Subcomponent
    {
        private string _subcomponentText = "";
        private HL7Component _parentComponent;

        public string Text
        {
            get { return _subcomponentText; }
            set { _subcomponentText = value; }
        }

        public bool IsEmpty { get { return String.IsNullOrEmpty(_subcomponentText); } }

        public HL7Subcomponent(string text) : this(text, null) { }
       
        public HL7Subcomponent(string text, HL7Component parentComponent)
        {
            _parentComponent = parentComponent;
            _subcomponentText = text;
        }

        public override string ToString()
        {
            return _subcomponentText;
        }
    }
}
