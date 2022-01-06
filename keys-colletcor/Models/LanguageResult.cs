using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keys_collector.Models
{
    public class LanguageResult
    {
        public LanguageResult(string keyword, List<Language> res)
        {
            Key = keyword;
            ResultList = res;
        }

        public string Key { get; set; }
        public List<Language> ResultList { get; set; }
    }
}
