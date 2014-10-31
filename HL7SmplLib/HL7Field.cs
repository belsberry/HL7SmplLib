using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
 * TODO if the field is repeated.  Break up the field and create a new internal array of fields for this field.
 * Behavior will be this:
 * if is repeated split on the repeat and create a new field to add to the internal set of fields.
 */

namespace HL7SmplLib
{
    public class HL7Field
    {
        #region data
        private string _fieldText;
        private HL7CharSet _charSet;
        private HL7Component[] _components;
        private HL7Field[] _repeatingFields;
        private HL7Segment _parentSegment;
        private bool _toSplit; //The primary purpose of this flag is to check for the character set in the MSH segment
        private bool _isRepeating;
        #endregion

        #region Properties

        /// <summary>
        /// The array of HL7Component objects populated by the loading methods.  If the 
        /// field is a repeating field then this will not have any indication as to the position of the components. 
        /// If you want to get a component by position, use the indexer of this object.  For example, if the field
        /// has 5 components that all repeat 3 times then the length of this particular array would be 15
        /// </summary>
        public HL7Component[] Components
        {
            get
            {
                if (!_isRepeating && _components != null) return _components;
                else if(_isRepeating) //Handle repeating fields
                {
                    List<HL7Component> comps = new List<HL7Component>();
                    foreach (HL7Field fi in _repeatingFields)
                    {
                        comps.AddRange(fi.Components);
                    } 
                    return comps.ToArray();
                }
                else return new HL7Component[]{ };//if the field is not repeating and the _components is null then return an empty array.
            }
        }
        
        /// <summary>
        /// The text of the field
        /// </summary>
        public string Text
        {
            get
            {
                if (_toSplit)
                {
                    if (!_isRepeating && _components != null)
                    {
                        string[] comp = new string[_components.Length];
                        for (int i = 0; i < comp.Length; i++) comp[i] = _components[i].Text;
                        return String.Join(_charSet.ComponentSeparator.ToString(), comp);
                    }
                    else if (_isRepeating)
                    {
                        string[] fields = new string[_repeatingFields.Length];
                        for (int i = 0; i < fields.Length; i++)
                        {
                            fields[i] = _repeatingFields[i].Text;

                        }
                        return String.Join(_charSet.RepeatChar.ToString(), fields);
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return _components[0].Text;
                }
            }
            set
            {
                SplitComponents(value);
            }
        }

        /// <summary>
        /// If the field is a repeating field (ex: has ~ characters) then this will contain the repeating HL7Field objects.
        /// </summary>
        public HL7Field[] Repeats { get { if (_isRepeating) return _repeatingFields; else return new HL7Field[] { this }; } }

        /// <summary>
        /// Indicates whether the field is a repeating field/the field contains repeats.
        /// </summary>
        public bool IsRepeating { get { return _isRepeating; } }

        /// <summary>
        /// Indicates whether or not the field has any relevant text in it.
        /// If the field has only separator characters then this will return true.
        /// </summary>
        public bool IsEmpty { 
            get 
            {
                bool isEmpty = true;
                foreach (HL7Component comp in Components)//use the accessor for repeating fields.
                {
                    if(!comp.IsEmpty)
                    {
                        isEmpty = false;
                        break;
                    }
                }
                return isEmpty;
            } 
        }

        #endregion

        #region contructors
        public HL7Field(string text, HL7CharSet hl7CharSet, bool split = true) : this(text, hl7CharSet, null, split) { }

        public HL7Field(string text, HL7CharSet hl7CharSet, HL7Segment parentSegment, bool split = true)
        {
            _parentSegment = parentSegment;
            _fieldText = text;
            _isRepeating = _fieldText.Contains(hl7CharSet.RepeatChar);
            _charSet = hl7CharSet;
            _toSplit = split;
            SplitComponents(_fieldText);
        }
        #endregion

        #region Public Methods
        /*
			 * Public Methods
			 */
        public override string ToString()
        {
            return Text;
        }

        public void AddComponent()
        {
            this.AddComponent(new HL7Component("", _charSet));
        }

        public void AddComponent(HL7Component component)
        {
            HL7Component[] temp = new HL7Component[_components.Length + 1];
            Array.Copy(_components, temp, _components.Length);
            temp[temp.Length - 1] = component;
            _components = temp;
        }

        /// <summary>
        /// Get the repeated field at a particular index
        /// </summary>
        /// <param name="ndx">The 0 based index of the repeat that is needed.</param>
        /// <returns>HL7Field</returns>
        public HL7Field GetRepeatNumber(int ndx)
        {
            
            if (ndx >= 0)
            {
                if (ndx < _repeatingFields.Length)
                {
                    return _repeatingFields[ndx];
                }
                else
                {
                    return new HL7Field("", _charSet, _toSplit);
                }
            }
            else
                throw new IndexOutOfRangeException("Index must be greater or equal to 0");
        }

        public HL7Field RepeatWhere(Func<HL7Field, bool> predicate)
        {
            if (_isRepeating)
            {
                HL7Field selected = _repeatingFields.FirstOrDefault(predicate);
                if (selected != null)
                    return selected;
                else
                    return new HL7Field("", _charSet);
            }
            else
            {
                if (predicate(this))
                {
                    return this;
                }
                else
                {
                    return new HL7Field("", _charSet);
                }
            }
        }

        public HL7Field[] AllRepeatsWhere(Func<HL7Field, bool> predicate)
        {
            if (_isRepeating)
            {
                HL7Field[] selected = _repeatingFields.Where(predicate).ToArray();
                return selected;
            }
            else
            {
                if (predicate(this))
                {
                    return new HL7Field[] { this };
                }
                else
                {
                    return new HL7Field[] { };
                }

            }
        }

        #endregion

        #region Private Methods
        /*
			 * Private Methods
			 */
        private void SplitComponents(string text)
        {
            _fieldText = text;
            if (!String.IsNullOrEmpty(text) && _toSplit)//If it is present and needs to be split up.
            {
                if (!_isRepeating)
                {
                    string[] temp = text.Split(_charSet.ComponentSeparator);
                    _components = new HL7Component[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                    {
                        _components[i] = new HL7Component(temp[i], _charSet, this);
                    }
                }
                else
                {
                    string[] tempRepeats = text.Split(_charSet.RepeatChar);
                    _repeatingFields = new HL7Field[tempRepeats.Length];
                    for (int i = 0; i < tempRepeats.Length; i++)
                    {
                        _repeatingFields[i] = new HL7Field(tempRepeats[i], _charSet, _toSplit);
                    }
                    
                }
            }
            else //Else leave it alone and don't split it.
            {
                _components = new HL7Component[1];
                _components[0] = new HL7Component(text, _charSet, this, false);
            }
        }
        #endregion

        #region indexers
        public HL7Component this[int index]
        {
            get
            {
                try
                {
                    if (_isRepeating && _repeatingFields.Length > 0)
                    {
                        return _repeatingFields[0][index];
                    }
                    else
                    {
                        return _components[index - 1];
                    }
                    throw new IndexOutOfRangeException();
                }
                catch (IndexOutOfRangeException)
                {
                    return new HL7Component("", this._charSet);
                }
                catch (NullReferenceException)
                {
                    throw new Exception("Error selecting component.  Component array not set.");
                }
                //if (index > 0)
                //{
                //    return Components[index - 1];
                //}
                //else if (index > Components.Length)
                //{
                //    throw new IndexOutOfRangeException("Selected component is out of range");
                //}
                //else
                //{
                //    throw new IndexOutOfRangeException("Invalid HL7 index for a component");
                //}
            }
        }
        #endregion

    }
}
