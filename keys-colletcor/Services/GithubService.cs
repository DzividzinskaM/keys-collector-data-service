using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using keys_collector.Models;
using Octokit;

namespace keys_collector.Services
{
	public class GithubService
	{
		private readonly GitHubClient client;
        private readonly UpdateService updateService;
        private Dictionary<string, List<IDisposable>> Connections;

        public GithubService(GitHubClient client, UpdateService updateService)
		{
			this.client = client;
            //updateService = new UpdateService();
            this.updateService = updateService;
            Connections = new Dictionary<string, List<IDisposable>>();

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

		public IObservable<IEnumerable<Repo>> ObservedRepos(string keyword, int pagesCount, string language, int perPage = 100)
        {
			var pages = new IObservable<SearchCodeResult>[pagesCount];
            for (int i = 1; i <= pagesCount; i++)
            {
                async Task<SearchCodeResult> functionAsync() => await GetPage(keyword, pagesCount, language, perPage);
                pages[i - 1] = Observable.FromAsync(functionAsync);
            }

            var modelLanguage = string.Empty;

			return Observable.Merge(pages).Take(pagesCount).Buffer(pagesCount)
                .Select(x => x.SelectMany(x => x.Items))
                .Select(x=>x.GroupBy(x => x.Repository.Id, (x) => (x.Repository, x.Name))
                            .Select(x => new Repo(x.FirstOrDefault().Repository.Name, 
                                                  x.FirstOrDefault().Repository.Url, 
                                                  x.Count(),
                                                  x.FirstOrDefault().Name.Split('.')[1] == null ? modelLanguage : x.FirstOrDefault().Name.Split('.')[1],
                                                  DateTime.Now)));
        }

        public async Task<long> GetKeyPages(RequestModel requestModel)
        {
            updateService.Add(requestModel.Keyword);

            var conn = Observable.Interval(TimeSpan.FromSeconds(requestModel.frequency))
                .Subscribe(x => ObservedRepos(requestModel.Keyword, requestModel.PageNumbers, requestModel.Language)
                                .Subscribe(x => updateService.Notify(requestModel.Keyword, x.ToList()) //.OrderByDescending(x => x.CoincidenceIndex)
            ));

            Connections.Add(requestModel.Keyword, new List<IDisposable>());
            Connections[requestModel.Keyword].Add(conn);

            return await Observable.Interval(TimeSpan.FromSeconds(requestModel.frequency)).FirstOrDefaultAsync();
        }
	}
}
