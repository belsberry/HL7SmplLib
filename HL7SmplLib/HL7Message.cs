using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/*
 * IMPROVEMENTS:
 *  Handle message groupings for easier processing.
 * 
 */


/*
 * TODO:
 *  1) Put comments on public code for the library.
 *  2) More testing on feature set.
 *  3) Polishing on the Networking features of the library.
 */


namespace HL7SmplLib
{
    /// <summary>
    /// HL7 Message object that parses the data. This class is meant to make use of the Indexer functionality for 
    /// simple syntax.
    /// </summary>
    /// <example><code>HL7Message message = new HL7Message();message["PID"][3][1][1].Text = "Here is the new text";</code></example>
    public class HL7Message
    {

        #region data
        private string _messageText;
        private HL7Segment[] _segments;
        private HL7CharSet _charSet;
        #endregion

        #region properties
        /// <summary>
        /// Array of segments found by the parser
        /// </summary>
        public HL7Segment[] Segments { get { return _segments; } }
        /// <summary>
        /// HL7 Character set found by the parser.
        /// </summary>
        public HL7CharSet CharSet { get { return _charSet; } set { _charSet = value; } }

        /// <summary>
        /// Text of the message
        /// </summary>
        public string Text
        {
            
            get
            {
                List<string> tempSegs = new List<string>();

                for (int i = 0; i < _segments.Length; i++)
                {
                    if (!_segments[i].IsEmpty)
                    {
                        tempSegs.Add(_segments[i].Text);
                    }
                }
                return String.Join(_charSet.SegmentSeparator, tempSegs.ToArray());
            }
            set
            {
                SplitSegments(value);
            }
        }
        #endregion

        #region constructors
        /*
         * Constructors
         */
        /// <summary>
        /// Create a new HL7Message object and parse the message at the same time.
        /// </summary>
        /// <param name="message">text of the HL7 message</param>
        public HL7Message(string message)
        {
            LoadHL7(message);
        }

        /// <summary>
        /// Create a new HL7Message object with no data.
        /// </summary>
        public HL7Message()
        {

        }
        #endregion

        #region public static
        /// <summary>
        /// Do a check to see if the message is in fact an HL7 message.
        /// </summary>
        /// <param name="message">message text</param>
        /// <returns>result of the check</returns>
        public static bool IsHL7(string message)
        {
            message = message.Trim();
            if ((message.Length > 15) && (message.Substring(0, 3).Equals("MSH") || message.StartsWith(HL7MLLP.VT + "MSH")))
            {
                return true;
            }
            else return false;
        }
        #endregion

        #region indexers
        /*
         * Indexers
         */
        //This only returns the first segment of this type as easy access.
        /// <summary>
        /// Indexer: returns the first HL7Segment with the given segment type string
        /// ex: message["PID"] would return the first PID segment
        /// If the segment is not found it returns null
        /// </summary>
        /// <param name="segmentType">ex: "PID"</param>
        /// <returns>HL7Segment</returns>
        public HL7Segment this[string segmentType]
        {
            get
            {
                foreach (HL7Segment seg in _segments)
                {
                    if (seg.Type.Equals(segmentType))
                    {
                        return seg;
                    }
                }
                return new HL7Segment("", this._charSet);
                //throw new HL7Exception(String.Format("Segment type {0} not found", segmentType));
            }
        }
        #endregion

        #region public
        /// <summary>
        /// Override that returns the text of the hl7 message
        /// </summary>
        /// <returns>hl7 message text</returns>
        public override string ToString()
        {
            return Text;
        }

        /// <summary>
        /// Load the text of the HL7 message
        /// </summary>
        /// <param name="message">HL7 message text</param>
        public void LoadHL7(string message)
        {
            _messageText = message.Trim();

            if (_messageText.Length > 15 && _messageText.Substring(0, 3).Equals("MSH"))
            {
                _charSet = new HL7CharSet(_messageText[3], _messageText[4], _messageText[7], _messageText[5]);   //Character set
                AnalyzeSegmentSeparator();
                SplitSegments(_messageText);                                                     //Split message
            }
            else
            {
                throw new HL7Exception("Invalid HL7 : " + _messageText);
            }
        }

        

        /// <summary>
        /// Returns the result of the function in the predicate.
        /// </summary>
        /// <param name="predicate">function that takes an HL7Segment and returns a boolean</param>
        /// <returns>The first HL7Segment that meets the boolean requirement or an empty segment if it can't find the segment.</returns>
        public HL7Segment FirstSegmentWhere(Func<HL7Segment, bool> predicate)
        {
            HL7Segment selectedSeg = _segments.FirstOrDefault(predicate);
            if (selectedSeg != null)
                return selectedSeg;
            else
                return new HL7Segment("", _charSet);
        }

        /// <summary>
        /// Select all of the segments that match the criteria in the function of the predicate.
        /// </summary>
        /// <param name="predicate">Func object that receives an HL7Segment and returns a boolean</param>
        /// <returns>Array of HL7Segments that match the criteria. If no segments match the criteria then this will be empty.</returns>
        public HL7Segment[] AllSegmentsWhere(Func<HL7Segment, bool> predicate)
        {
            IEnumerable<HL7Segment> selectedSegs = _segments.Where(predicate);
            return selectedSegs.ToArray();
        }

        /// <summary>
        /// Get the index of the first Segment of the type specified
        /// </summary>
        /// <param name="segmentType">ex: "PID"</param>
        /// <returns>index of the first segment of that type. -1 if not successful</returns>
        public int GetIndexOfSegment(string segmentType)
        {
            return GetIndexOfSegment(segmentType, 1);
        }

        /// <summary>
        /// Gets the index of the segment based on type and skips any number of segments needed
        /// </summary>
        /// <param name="segmentType">ex: "OBX"</param>
        /// <param name="segmentCount">The count of the segment.  If you are looking for the 3rd OBX then put 3 here.</param>
        /// <returns>index of the segment or -1 if not successful</returns>
        public int GetIndexOfSegment(string segmentType, int segmentCount)
        {
            if (segmentCount < 0)
            {
                return this.GetLastIndexOfSegment(segmentType, segmentCount);
            }
            else
            {
                for (int i = 0; i < _segments.Length; i++)
                {
                    if (_segments[i].Type.Equals(segmentType) && segmentCount > 1)
                    {
                        segmentCount--;
                    }
                    else if (_segments[i].Type.Equals(segmentType) && segmentCount == 1)
                    {
                        return i;
                    }
                }
                return -1;
            }
        }

        /// <summary>
        /// Get the last index of a particular segment type
        /// </summary>
        /// <param name="segmentType">Type of segment to check for</param>
        /// <returns>0 based index of the last instance of a type of segment or -1 if the segment can't be found.</returns>
        public int GetLastIndexOfSegment(string segmentType)
        {
            return GetLastIndexOfSegment(segmentType, -1); //Last instance of the segment
        }
        
        /// <summary>
        /// Get the last index of a particular segment type but walk backward by the negative count
        /// </summary>
        /// <param name="segmentType">Type of segment to get the count for</param>
        /// <param name="negativeSegCount">The negative count of the segment. EX: -2 would be the second from the end.</param>
        /// <returns>0 based index of the segment or -1 if the segment can't be found</returns>
        public int GetLastIndexOfSegment(string segmentType, int negativeSegCount)
        {
            for (int i = _segments.Length - 1; i > 0; i--)
            {
                if (_segments[i].Type == segmentType && negativeSegCount < -1)
                {
                    negativeSegCount++;
                }
                else if (_segments[i].Type == segmentType && negativeSegCount >= -1)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Get the first segment of a type after the start index
        /// </summary>
        /// <param name="segmentType">Type string such as "PID"</param>
        /// <param name="startIndex">index to start the search</param>
        /// <returns>HL7Segment object of the type specified.</returns>
        public HL7Segment GetOneSegmentByType(string segmentType, int startIndex)
        {
            for (int i = startIndex; i < Segments.Length; i++)
            {
                if (Segments[i].Type.Equals(segmentType))
                {
                    return Segments[i];
                }
            }
            throw new HL7Exception(String.Format("There are no more segments of that type after index: {0}", startIndex));
        }

        
        //Returns an array of segments with a specific type.
        /// <summary>
        /// Get all segments of the specified type.
        /// </summary>
        /// <param name="segmentType">Type of segments to return such as "PID"</param>
        /// <returns>Array of HL7Segments of the specified type.</returns>
        public HL7Segment[] GetSegmentsByType(string segmentType)
        {
            List<HL7Segment> segs = new List<HL7Segment>();
            foreach (HL7Segment seg in Segments)
            {
                if (seg.Type.Equals(segmentType))
                {
                    segs.Add(seg);
                }
            }
            return segs.ToArray();
        }


        /// <summary>
        /// Get all segments of a particular type after a given index in the message
        /// </summary>
        /// <param name="segmentType">Type of segments to return such as "PID"</param>
        /// <param name="startIndex">Index to start the search</param>
        /// <returns>Array of HL7Segment objects.</returns>
        public HL7Segment[] GetSegmentsByType(string segmentType, int startIndex)
        {
            List<HL7Segment> segs = new List<HL7Segment>();
            for (int i = startIndex; i < Segments.Length; i++)
            {
                if (Segments[i].Type.Equals(segmentType))
                {
                    segs.Add(Segments[i]);
                }
            }
            return segs.ToArray();
        }

        /// <summary>
        /// Add a segment to the end of the message
        /// </summary>
        /// <param name="seg">The HL7Segment object to add to the message</param>
        public void AddSegment(HL7Segment seg)
        {

            //Add Null logic here
            HL7Segment[] tempSegs = new HL7Segment[1];
            if (_segments == null) _segments = new HL7Segment[1];
            else
            {
                tempSegs = new HL7Segment[_segments.Length + 1];
                Array.Copy(_segments, tempSegs, _segments.Length);
            }
            tempSegs[tempSegs.Length - 1] = seg;
            _segments = tempSegs;
        }

        /// <summary>
        /// Insert a HL7Segment object after the first segment of the requested type.
        /// </summary>
        /// <param name="seg">Segment object to insert</param>
        /// <param name="segmentToInsertAfter">Segment type to insert after</param>
        public void InsertSegment(HL7Segment seg, string segmentToInsertAfter)
        {
            this.InsertSegment(seg, segmentToInsertAfter, 1);//Call the same method but insert after first
        }

        /// <summary>
        /// Insert an HL7Segment object after the segment of the requested type at the specified count
        /// </summary>
        /// <param name="seg">HL7Segment object to insert</param>
        /// <param name="segmentToInsertAfter">Segment type to insert after</param>
        /// <param name="segmentCount">Count of the segment to insert after. Setting this to -1 will count backward.</param>
        public void InsertSegment(HL7Segment seg, string segmentToInsertAfter, int segmentCount)
        {
            int ndx = this.GetIndexOfSegment(segmentToInsertAfter, segmentCount);
            if (ndx > 0)
            {
                this.InsertSegment(seg, ndx + 1);
            }
            else
                this.AddSegment(seg);
        }

        /// <summary>
        /// Insert a segment by string at a position after the count of the segment
        /// </summary>
        /// <param name="seg">Segment text</param>
        /// <param name="segmentToInsertAfter">Type of segment to insert after</param>
        /// <param name="segmentCount">Count of te segment to insert after.</param>
        public void InsertSegment(string seg, string segmentToInsertAfter, int segmentCount)
        {
            HL7Segment segObj = new HL7Segment(seg, this._charSet);
            this.InsertSegment(segObj, segmentToInsertAfter, segmentCount);
        }

        /// <summary>
        /// Insert a segment by string at a position after the first instance of another segment type.
        /// </summary>
        /// <param name="seg">Segment Text</param>
        /// <param name="segmentToInsertAfter">Type of segment to insert after</param>
        public void InsertSegment(string seg, string segmentToInsertAfter)
        {
            this.InsertSegment(seg, segmentToInsertAfter, 1);
        }

        /// <summary>
        /// Insert an HL7Segment object at a specific index 
        /// </summary>
        /// <param name="seg">object to insert</param>
        /// <param name="index">0 based index to insert the segment. The index is the row.</param>
        public void InsertSegment(HL7Segment seg, int index)
        {
            HL7Segment[] tempArray = new HL7Segment[_segments.Length + 1];
            if (index > tempArray.Length) this.AddSegment(seg);
            else
            {
                for (int i = 0; i < index; i++)
                {
                    tempArray[i] = _segments[i];
                }
                tempArray[index] = seg;
                for (int i = index + 1; i < tempArray.Length; i++)
                {
                    tempArray[i] = _segments[i - 1];
                }
                _segments = tempArray;
            }
        }

        public void InsertSegment(string seg, int index)
        {
            HL7Segment segObj = new HL7Segment(seg, this._charSet);
            this.InsertSegment(segObj, index);
        }

        public HL7Segment GetSegmentByLocation(string segmentLocation)
        {
            HL7Location loc = HL7Location.Parse(segmentLocation);
            return GetSegmentByLocation(loc);
        }

        public HL7Segment GetSegmentByLocation(HL7Location loc)
        {
            return this[loc.Segment];
        }

        public HL7Field GetFieldByLocation(string fieldLocation)
        {
            HL7Location loc = HL7Location.Parse(fieldLocation);
            return GetFieldByLocation(loc);
        }

        public HL7Field GetFieldByLocation(HL7Location loc)
        {
            return this[loc.Segment][loc.Field];
        }

        public HL7Component GetComponentByLocation(string compLocation)
        {
            HL7Location loc = HL7Location.Parse(compLocation);
            return GetComponentByLocation(loc);
        }

        public HL7Component GetComponentByLocation(HL7Location loc)
        {
            return this[loc.Segment][loc.Field][loc.Component];
        }

        public HL7Subcomponent GetSubcomponentByLocation(string subLocation)
        {
            HL7Location loc = HL7Location.Parse(subLocation);
            return GetSubcomponentByLocation(loc);
        }
        public HL7Subcomponent GetSubcomponentByLocation(HL7Location loc)
        {
            return this[loc.Segment][loc.Field][loc.Component][loc.Subcomponent];
        }

        #endregion

        #region private
        private void SplitSegments(string hl7Message)
        {
            _messageText = hl7Message;
            string[] mySegments = hl7Message.Split(_charSet.SegmentSeparator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            mySegments = CleanLineEndings(mySegments);
            
            _segments = new HL7Segment[mySegments.Length];//TODO try to remove empty entries after this is processed.
            //segments = CleanLineEndings(segments);
            for (int i = 0; i < _segments.Length; i++)
            {
                if (!String.IsNullOrEmpty(mySegments[i]))
                {
                    _segments[i] = new HL7Segment(mySegments[i], _charSet, this);
                }
            }
        }

        private string[] CleanLineEndings(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++ )
            {
                lines[i] = lines[i].Trim('\r', '\n');
            }
            return lines;
        }

        private void AnalyzeSegmentSeparator()
        {
            if (!String.IsNullOrEmpty(_messageText) && _charSet != null)
            {
                int ndxCarriageReturn = _messageText.IndexOf('\r');
                if (ndxCarriageReturn > 0)
                {
                    if (_messageText.Length > ndxCarriageReturn + 1 && _messageText[ndxCarriageReturn + 1] == '\n')
                    {
                        _charSet.SetSegmentSeparator(LineEnding.CarriageReturnLineFeed);
                    }
                    else
                    {
                        _charSet.SetSegmentSeparator(LineEnding.CarriageReturn);
                    }
                }
                else
                {
                    _charSet.SetSegmentSeparator(LineEnding.LineFeed);
                }
            }
        }
        #endregion
    }
    
    //public enum InsertSegmentOption
    //{
    //    AfterLastSegmentOfSameType, BeforeFirstSegmentOfSameType
    //}
}
