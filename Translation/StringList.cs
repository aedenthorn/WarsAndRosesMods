using System;
using System.Collections.Generic;

namespace Translation
{
    [Serializable]
    public class StringList
    {
        public List<string> list = new List<string>();

        public StringList(List<string> allStrings)
        {
            list = allStrings;
        }
    }
}