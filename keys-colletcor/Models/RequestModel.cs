using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keys_collector.Models
{
    public class RequestModel
    {
        public string Keyword { get; set; }
        public int PageNumbers { get; set; } = 1;
        public string Language { get; set; }
        public int frequency = 10;
    }
}
