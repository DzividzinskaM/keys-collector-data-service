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
            try
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
            catch (Exception)
            {
                return default;
            }
		}

		public IObservable<IEnumerable<Repo>> ObservedRepos(string keyword, int pagesCount, string language, int perPage = 100)
        {
			var pages = new IObservable<SearchCodeResult>[pagesCount];
            for (int i = 1; i <= pagesCount; i++)
            {
                async Task<SearchCodeResult> functionAsync() => await GetPage(keyword, pagesCount, language, perPage);
                pages[i - 1] = Observable.FromAsync(functionAsync);
            }

            var notNullPagesCount = pages.Where(x => x != null).Count();
            var modelLanguage = string.Empty;

            try
            {
                return Observable.Merge(pages).Take(notNullPagesCount).Buffer(notNullPagesCount)//.Take(pagesCount).Buffer(pagesCount)
                .Select(x => x.SelectMany(x => x == null ? default : x.Items))
                .Select(x => x.GroupBy(x => x.Repository.Id, (x) => (x.Repository, x.Name))
                            .Select(x => new Repo(x.FirstOrDefault().Repository.Name,
                                                    x.FirstOrDefault().Repository.Url,
                                                    x.Count(),
                                                    x.FirstOrDefault().Name.Split('.')[1] ?? modelLanguage,
                                                    DateTime.Now)));
            }
            catch (Exception)
            {
                return default;
            }
        }

        public async Task<List<IDisposable>> GetKeyPages(RequestModel requestModel)
        {
            updateService.Add(requestModel.Keyword);

            try
            {
                var conn = Observable.Interval(TimeSpan.FromSeconds(requestModel.frequency))
                    .Subscribe(x => ObservedRepos(requestModel.Keyword, requestModel.PageNumbers, requestModel.Language)
                                    .Subscribe(x => updateService.Notify(requestModel.Keyword, x.ToList()) //.OrderByDescending(x => x.CoincidenceIndex)
                ));

                AddToDictionary(Connections, requestModel.Keyword, new List<IDisposable>(), conn);
            }
            catch(Exception){

                return default;
            }

            //Connections.Add(requestModel.Keyword, new List<IDisposable>());
            //Connections[requestModel.Keyword].Add(conn);

            //return await Observable.Interval(TimeSpan.FromSeconds(requestModel.frequency)).FirstOrDefaultAsync();

            return Connections[requestModel.Keyword];
            //return updateService.Repos[requestModel.Keyword].ToList();
        }

       
        public void AddToDictionary(Dictionary<string, List<IDisposable>> dict, string key, List<IDisposable> value, IDisposable additionalvalue=null)
        {
            if (dict.ContainsKey(key))
            {
                foreach (var item in value)
                    dict[key].Add(item);
            }
            else
            {
                dict.Add(key, value);
            }

            if (additionalvalue != null)
                dict[key].Add(additionalvalue);
        }
	}
}
