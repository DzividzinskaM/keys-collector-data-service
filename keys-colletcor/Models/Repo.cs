using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keys_collector.Models
{
    public class Repo
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public int CoincidenceIndex { get; set; }
        public string LanguageName { get; set; }
        public DateTime Date { get; set; }

        public Repo(string name, string url, int index, string language, DateTime date)
        {
            Name = name; Url = url; CoincidenceIndex = index; LanguageName = language; Date = date;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Repo r = (Repo)obj;
                return (Name == r.Name) && (Url == r.Url) && (LanguageName == r.LanguageName)
                    && (CoincidenceIndex == r.CoincidenceIndex) && (Date == r.Date);
            }
        }
    }
}
