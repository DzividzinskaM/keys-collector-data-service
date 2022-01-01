using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keys_collector.Models
{
    public class RepositoryResult
    {
        public string Key { get; set; }
        public List<Repo> ResultList { get; set; }
    }
}
