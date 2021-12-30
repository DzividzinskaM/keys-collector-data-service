using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keys_colletcor.Models
{
    public class Repository
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public int CoincidenceIndex { get; set; }
        public string LanguageName { get; set; }
        public DateTime Date { get; set; }
    }
}
