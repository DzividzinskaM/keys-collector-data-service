using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using keys_colletcor.Models;
using Octokit;

namespace keys_collector.Services
{
	public class GithubService
	{
		private readonly GitHubClient client;

		public GithubService(GitHubClient client)
		{
			this.client = client;
		}

		public async Task<SearchCodeResult> GetPage(string keyword, int page, string language, int perPage = 100)
		{
            var searchRequest = new SearchCodeRequest(keyword)
            {
                SortField = CodeSearchSort.Indexed,
                Language = Enum.Parse<Octokit.Language>(language),
                Page = page,
                PerPage = perPage
            };
            var res = await client.Search.SearchCode(searchRequest);
			return res;
		}

		public IObservable<List<Repo>> ObservedRepos(string keyword, int pagesCount, string language, int perPage = 100)
        {
			var pages = new IObservable<SearchCodeResult>[pagesCount];
            for (int i = 1; i <= pagesCount; i++)
            {
                async Task<SearchCodeResult> functionAsync() => await GetPage(keyword, pagesCount, language, perPage);
                pages[i - 1] = Observable.FromAsync(functionAsync);
            }

			return (IObservable<List<Repo>>)Observable.Merge(pages).Take(pagesCount).Buffer(pagesCount)
                .Select(x => x.SelectMany(x => x.Items))
                .Select(x=>x.GroupBy(x => x.Repository.Id, (x) => (x.Repository, x.Name))
                            .Select(x => new Repo(x.FirstOrDefault().Repository.Name, 
                                                  x.FirstOrDefault().Repository.Url, 
                                                  x.Count(), 
                                                  x.FirstOrDefault().Name.Split('.')[1], 
                                                  DateTime.Now)));
        }
	}
}
