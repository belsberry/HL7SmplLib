using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7SmplLib
{
    /// <summary>
    /// Object that encapsulates the data associated with a standard HL7Segment in a message.
    /// Primary usage will be in conjunction with the HL7Message methods.
    /// Can be a standalone object if needed. 
    /// </summary>
    public class HL7Segment
    {
        #region data
        private string _segmentText ="";
        private string _type ="";
        private HL7CharSet _charSet;
        private HL7Field[] _fields;
        private HL7Message _parentMessage = null;
        #endregion

        #region properties
        /// <summary>
        /// Type of the segment based on what was parsed.  Ex: "PID"
        /// </summary>
        public string Type { get { return _type; } }
        /// <summary>
        /// Array of parsed HL7Field objects 
        /// </summary>
        public HL7Field[] Fields { get { return _fields; } }

        /// <summary>
        /// Does the segment actually have any data in it? This will return false in situations where the text has been set to nothing.
        /// </summary>
        public bool IsEmpty { get {
            bool isEmpty = true;
            foreach (HL7Field field in _fields)
            {
                if (!field.IsEmpty)
                {
                    isEmpty = false;
                    break;
                }
                
            }
            return isEmpty; 
        } }

        /// <summary>
        /// Get the text of the segment
        /// </summary>
        public string Text
        {
            get
            {
                if (!Type.Equals("MSH"))
                {
                    string[] temp = new string[Fields.Length];
                    for (int i = 0; i < Fields.Length; i++) temp[i] = Fields[i].Text;
                    return String.Join(_charSet.FieldSeparator.ToString(), temp);
                }
                else
                {
                    string[] temp = new string[Fields.Length - 2];                      //Make a new array that is 2 less than the current
                    temp[0] = Fields[0].Text + Fields[1].Text + Fields[2].Text;         //put together the delimiters and special segments
                    for (int i = 1; i < temp.Length; i++) temp[i] = Fields[i + 2].Text; //add the rest of the fields to the array.
                    return String.Join(_charSet.FieldSeparator.ToString(), temp);        //join the fields together into a string.
                }
            }
            set
            {
                SplitFields(value);
            }
        }

        /// <summary>
        /// Get the currently used HL7 character set
        /// </summary>
        public HL7CharSet CharSet { get { return _charSet; } }
        #endregion

        #region contructors
        /// <summary>
        /// Create a new object with the segment data specified
        /// </summary>
        /// <param name="segment">data for the segment</param>
        public HL7Segment(string segment) : this(segment, null) { }


        /// <summary>
        /// Create a new object with the data and a specific characterset
        /// </summary>
        /// <param name="segment">data for the segment</param>
        /// <param name="set">HL7CharSet object for parsing</param>
        public HL7Segment(string segment, HL7CharSet set) : this(segment, set, null) { }

        /// <summary>
        /// Creates a new object with the data, charset, and parent HL7Message.
        /// </summary>
        /// <param name="segment">Segment Data</param>
        /// <param name="set">Character Set</param>
        /// <param name="parentMessage">Parent message object</param>
        public HL7Segment(string segment, HL7CharSet set, HL7Message parentMessage)
        {
            _parentMessage = parentMessage;
            _segmentText = segment.TrimStart();
            if (set == null) set = ParseCharSet(_segmentText);
            if (_segmentText.Length > 3) _type = _segmentText.Substring(0, 3);
            _charSet = set;
            SplitFields(_segmentText);
        }
        #endregion

        #region public
        

        /// <summary>
        /// Gets the value in Text
        /// </summary>
        /// <returns>HL7Segment data</returns>
        public override string ToString()
        {
            return Text;
        }

        /// <summary>
        /// Add a field to the end of the segment that is empty
        /// </summary>
        public void AddField()
        {
            this.AddField(new HL7Field("", _charSet));
        }


        /// <summary>
        /// Add a field that has already been created to the end of the segment
        /// </summary>
        /// <param name="field">previously created field object</param>
        public void AddField(HL7Field field)
        {
            HL7Field[] temp = new HL7Field[_fields.Length + 1]; //make an array that is one greater 
            Array.Copy(_fields, temp, _fields.Length);        //copy all fields to the new array
            temp[temp.Length - 1] = field;                  //add the new field to the end
            _fields = temp;                                  //assign fields to the new array.
        }

        /// <summary>
        /// Expand the segment to a specific length
        /// </summary>
        /// <param name="location">HL7Location object that specifies the location to reach</param>
        public void ExpandToLocation(HL7Location location)
        {
            while (this.Fields.Length <= location.Field) this.AddField(); //Field index is 0 based since technically first field is the segment ID

            while (this[location.Field].Components.Length < location.Component)
                this[location.Field].AddComponent(); //Index starts at 1 in HL7

            while (this[location.Field][location.Component].Subcomponents.Length < location.Subcomponent)
                this[location.Field][location.Component].AddSubcomponent(); //Index starts at 1 in HL7
        }

        /// <summary>
        /// Expand the segment to a specific length
        /// </summary>
        /// <param name="strLocation">Location string in the format such as "PID.3.1.1".  Must specify a segment name to hold the place even thought it will not be used.</param>
        public void ExpandToLocation(string strLocation)
        {
            HL7Location loc = HL7Location.Parse(strLocation);
            ExpandToLocation(loc);
        }

        #endregion

        #region private
        private void SplitFields(string text)
        {
            _segmentText = text;
            string[] myFields;
            if (Type.Equals("MSH"))
            {
                string[] temp = text.Split(_charSet.FieldSeparator);
                myFields = new string[temp.Length + 1];
                myFields[0] = temp[0];
                myFields[1] = _charSet.FieldSeparator.ToString();
                for (int i = 2; i < myFields.Length; i++)
                {
                    myFields[i] = temp[i - 1];
                }
            }
            else
            {
                myFields = text.Split(_charSet.FieldSeparator);
            }
            _fields = new HL7Field[myFields.Length];
            for (int i = 0; i < 3 && i < _fields.Length; i++)
            {
                if (Type.Equals("MSH"))
                {
                    if (i == 1 || i == 2)
                    {
                        _fields[i] = new HL7Field(myFields[i], _charSet, this,  false);
                    }
                    else
                    {
                        _fields[i] = new HL7Field(myFields[i], _charSet, this);
                    }
                }
                else
                {
                    _fields[i] = new HL7Field(myFields[i], _charSet, this);
                }
            }
            for (int i = 3; i < myFields.Length; i++)
            {

                _fields[i] = new HL7Field(myFields[i], _charSet, this);
            }
        }
        private HL7CharSet ParseCharSet(string segment)
        {
            HL7CharSet charSet;
            if (segment.StartsWith("MSH") && segment.Length > 15) //If MSH then get the Segment from the Header charset
            {
                charSet = new HL7CharSet(segment[3], segment[4], segment[7], segment[5]);
            }
            else
            {
                charSet = new HL7CharSet('|', '^', '&', '~'); //Set to default
            }
            return charSet;
        }
        #endregion

        #region indexers
        /// <summary>
        /// Get the HL7Field object at a specific index
        /// </summary>
        /// <param name="index">index of the field in the segment</param>
        /// <returns>HL7Field object at the index.  Returns an empty field if the index is out of range.</returns>
        public HL7Field this[int index]
        {
            get
            {
                try
                {
                    return _fields[index];
                }
                catch (IndexOutOfRangeException)
                {
                    //throw new HL7Exception(String.Format("{0} does not have a field {1} for this message", this.Type, index));
                    return new HL7Field("", this._charSet);
                }
            }
        }
        #endregion
    }
}
