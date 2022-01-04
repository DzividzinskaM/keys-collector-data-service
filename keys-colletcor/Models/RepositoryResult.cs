using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keys_collector.Models
{
    public class RepositoryResult
    {

        public RepositoryResult(string keyword, List<Repo> list)
        {
            Key = keyword;
            ResultList = list;
        }

        public string Key { get; set; }
        public List<Repo> ResultList { get; set; }
    }
}
