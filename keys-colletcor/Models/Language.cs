using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keys_collector.Models
{
    public class Language
    {

        public Language(string key, int v)
        {
            LanguageName = key;
            CoincidenceIndex = v;
        }

        public string LanguageName { get; set; }
        public int CoincidenceIndex { get; set; }
    }
}
