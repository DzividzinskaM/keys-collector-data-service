using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keys_collector.Models
{
    public class Repo:IComparable
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

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Url, LanguageName, CoincidenceIndex);
        }

        public int CompareTo(object obj)
        {
            if (string.Compare(Name,((Repo)obj).Name)>0)
            {
                return 1;
            }
            else if (string.Compare(Name, ((Repo)obj).Name) < 0)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}
