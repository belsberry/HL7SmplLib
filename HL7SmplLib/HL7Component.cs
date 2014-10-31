using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7SmplLib
{
    /// <summary>
    /// HL7 Component type.  Typically separated in an HL7 message with a ^ character.
    /// Use the indexer syntax to easily access the Subcomponents.
    /// </summary>
    public class HL7Component
    {
        private string _componentText;
        private HL7CharSet _charSet;
        private HL7Subcomponent[] _subcomponents;
        private bool _toSplit;
        private HL7Field _parentField;

        #region Properties
        /*
			 * Properties
			 */
        /// <summary>
        /// Text for the Component
        /// </summary>
        public string Text
        {
            get
            {
                if (_toSplit)
                {
                    string[] subText = new string[_subcomponents.Length];
                    for (int i = 0; i < subText.Length; i++) subText[i] = _subcomponents[i].Text;
                    return String.Join(_charSet.SubComponentSeparator.ToString(), subText);
                }
                else
                {
                    return _subcomponents[0].Text;
                }
            }
            set
            {
                _componentText = value;
                splitSubcomponents(value);
            }
        }

        /// <summary>
        /// Is the Component empty?
        /// </summary>
        public bool IsEmpty 
        { 
            get 
            {
                bool isEmpty = true;
                foreach (HL7Subcomponent sub in _subcomponents)
                {
                    if (!sub.IsEmpty)
                    {
                        isEmpty = false;
                        break;
                    }
                }
                return isEmpty;
            } 
        }

        /// <summary>
        /// Array of parsed HL7Subcomponent objects
        /// </summary>
        public HL7Subcomponent[] Subcomponents
        {
            get
            {
                return _subcomponents;
            }
        }
        #endregion

        /*
			 * Constructor
			 */
        /// <summary>
        /// HL7Component object
        /// </summary>
        /// <param name="text">text of the component</param>
        /// <param name="set">HL7 character set</param>
        /// <param name="split">Onl</param>
        public HL7Component(string text, HL7CharSet set, bool split = true) : this(text, set, null, split) { } 

        public HL7Component(string text, HL7CharSet set, HL7Field parentField, bool split = true)
        {
            _parentField = parentField;
            _componentText = text;
            _charSet = set;
            _toSplit = split;
            splitSubcomponents(text);
        }


        #region Public Methods
        public override string ToString()
        {
            return Text;
        }

        public void AddSubcomponent()
        {
            this.AddSubcomponent(new HL7Subcomponent(""));
        }

        public void AddSubcomponent(HL7Subcomponent subcomp)
        {
            HL7Subcomponent[] temp = new HL7Subcomponent[_subcomponents.Length + 1];
            Array.Copy(_subcomponents, temp, _subcomponents.Length);
            temp[temp.Length - 1] = subcomp;
            _subcomponents = temp;
        }
        #endregion

        #region Private Methods
        /*
			 * Private Methods
			 */
        private void splitSubcomponents(string text)
        {
            _componentText = text;
            if (!String.IsNullOrEmpty(text) && _toSplit)
            {
                string[] temp = text.Split(_charSet.SubComponentSeparator);
                _subcomponents = new HL7Subcomponent[temp.Length];
                for (int i = 0; i < temp.Length; i++)
                {
                    _subcomponents[i] = new HL7Subcomponent(temp[i], this);
                }
            }
            else
            {
                _subcomponents = new HL7Subcomponent[1];
                _subcomponents[0] = new HL7Subcomponent(text, this);
            }
        }
        #endregion

        /*
			 * Indexer
			 */
        public HL7Subcomponent this[int subIndex]
        {
            get
            {
                if (subIndex > 0 && subIndex <= _subcomponents.Length)
                {
                    return _subcomponents[subIndex - 1];
                }
                else if (subIndex < 1)
                {
                    throw new IndexOutOfRangeException("Invalid HL7 index. Cannot be less than 1.");
                }
                else
                {
                    return new HL7Subcomponent("");
                    //throw new IndexOutOfRangeException("Subcomponent does not exist. Index out of range.");
                }
            }
        }
    }
}
