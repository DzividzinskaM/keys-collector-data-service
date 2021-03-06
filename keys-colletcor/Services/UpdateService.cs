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
        public Dictionary<string, BehaviorSubject<List<Repo>>> Last10Repos = new Dictionary<string, BehaviorSubject<List<Repo>>>();

        public Dictionary<string, List<Repo>> Current = new Dictionary<string, List<Repo>>();
        public void Add(string key)
        {
            if (!Repos.ContainsKey(key))
                Repos.Add(key, new Subject<List<Repo>>());

            if (!Last10Repos.ContainsKey(key))
                Last10Repos.Add(key, new BehaviorSubject<List<Repo>>(new List<Repo>()));
        }

        public void NotifyRepos(string key, List<Repo> search)
        {
            Repos[key].OnNext(search);
        }

        public void NotifyLast10(string key, List<Repo> search)
        {
            Last10Repos[key].OnNext(search);
        }

        public List<Repo> GetDistinctRepos(string keyword, List<Repo> repos)
        {
            if (!Current.ContainsKey(keyword))
            {
                Current.Add(keyword,repos);
                return repos;
            }
            var newRepos = new List<Repo>();
            foreach (var repo in repos.Where(repo => !Current[keyword].Contains(repo)))
            {
                Current[keyword].Add(repo);
                newRepos.Add(repo);
            }

            return newRepos;
        }
    }
}
