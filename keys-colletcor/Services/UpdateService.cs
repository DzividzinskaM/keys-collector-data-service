using keys_collector.Models;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace keys_collector.Services
{
    public class UpdateService
    {
        public Dictionary<string, Subject<List<Repo>>> Repos = new Dictionary<string, Subject<List<Repo>>>();
        public void Add(string key)
        {
            Repos.Add(key, new Subject<List<Repo>>());
        }

        public void Notify(string key, List<Repo> search)
        {
            Repos[key].OnNext(search);
        }
    }
}
