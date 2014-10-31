using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7SmplLib
{
    static class ExtensionMethods
    {
        public static string[] SplitByNonAlphaNumerics(this string input)
        {
            List<string> strings = new List<string>();
            int lastIndex = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (!Char.IsLetterOrDigit(input[i]))
                {
                    string stringToAdd = input.Substring(lastIndex, i - lastIndex);
                    if (!String.IsNullOrEmpty(stringToAdd)) strings.Add(stringToAdd);
                    lastIndex = i + 1;
                }
            }
            string lastString = null;
            if (lastIndex < input.Length) lastString = input.Substring(lastIndex);
            if (!String.IsNullOrEmpty(lastString)) strings.Add(lastString);
            return strings.ToArray();
        }
    }
}
