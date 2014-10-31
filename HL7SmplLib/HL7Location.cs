using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using HL7SmplLib.Text;


namespace HL7SmplLib
{
    public enum HL7LocationParsingMode
    {
        /// <summary>
        /// Truncate Mode will shorten the Segment name to the standard 3 character segment name.
        /// This will also throw an Exception if the Segment name is less than 3 characters
        /// </summary>
        Truncate,
        /// <summary>
        /// Validate Mode will throw an exception if the Segment name is found to have more than the 3 standard characters
        /// </summary>
        Validate
    }
    public class HL7Location
    {
        public string Segment { get; set; }
        public int Field { get; set; }
        public int Component { get; set; }
        public int Subcomponent { get; set; }
        public int SegmentIteration { get; set; }
        public bool HasSegmentIteration { get; set; }        

        public HL7Location()
        {
            Segment = "";
            Field = 0;
            Component = 0;
            Subcomponent = 0;
            HasSegmentIteration = false;
        }

        public static HL7Location Parse(string location, HL7LocationParsingMode parsingMode = HL7LocationParsingMode.Truncate)
        {
            HL7Location loc = new HL7Location();
            string[] pieces = location.SplitByNonAlphaNumerics();
            loc.Field = 1;
            loc.Component = 1;
            loc.Subcomponent = 1;
            try
            {
                if (pieces.Length > 3) loc.Subcomponent = Int32.Parse(pieces[3]);
                if (pieces.Length > 2) loc.Component = Int32.Parse(pieces[2]);
                if (pieces.Length > 1) loc.Field = Int32.Parse(pieces[1]);
                if (pieces.Length > 0) loc.Segment = pieces[0];
                loc = CheckSegmentName(loc, parsingMode);
            }
            catch(Exception ex)
            {
                throw new HL7Exception(String.Format("Invalid HL7 Location, {0}  Must be in a format such as MSH.1.1.1", location));
            }
            return loc;
        }

        private static HL7Location CheckSegmentName(HL7Location loc, HL7LocationParsingMode parsingMode)
        {
            switch (parsingMode)
            {
                case HL7LocationParsingMode.Truncate:
                    loc.Segment = loc.Segment.Substring(0, 3); //this with throw an exception if there are less than three characters
                    break;
                case HL7LocationParsingMode.Validate:
                    if (loc.Segment.Length == 3)
                        throw new HL7Exception(String.Format("Invalid segment name {0}. Must be 3 characters.", loc.Segment));
                    break;
                default:
                    break;
            }
            return loc;
        }

        //This may be more trouble to get this right than thought.  I want to be as efficient as possible.
        //TODO look into common string tokenization techniques
        //Here is a possibility.  iterate through and if the character is non numeric then break of the chunk.  If it is a [ then look for the next ] and put it together
        public static HL7Location Parse(string location, bool parseWithIterationNum)
        {
            if (parseWithIterationNum || !location.Contains('[') || !location.Contains(']') || location.IndexOf('[') > location.IndexOf(']'))
                throw new HL7Exception(String.Format("{0} cannot be parsed with iteration. Must be in the format OBX[2].1.1.1"));

            throw new NotImplementedException();
        }

        //TODO check into how to check for repeating segments
        public override string ToString()
        {
            string toReturn = "";

            //Handle Segment
            if (!String.IsNullOrEmpty(Segment)) toReturn += Segment;    
            else return ""; //This is no good without a Segment assigned.       
            if (HasSegmentIteration) toReturn += "[" + SegmentIteration + "]";
            toReturn += "." + Field;
            toReturn += "." + Component;
            toReturn += "." + Subcomponent;

            return toReturn;                
        }
        
    }
}
