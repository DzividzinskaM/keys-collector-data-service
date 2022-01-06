using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using keys_collector.Models;
using Microsoft.Extensions.DependencyInjection;
using Octokit;

namespace keys_collector.Services
{
	public class GithubService
	{
		private GitHubClient client;
        private readonly UpdateService updateService;
        private readonly Dictionary<string, List<IDisposable>> Connections;
        public readonly List<RepositoryResult> NewRepositoryResultsLogger;
        public readonly List<LanguageResult> RecentLanguages;
        public List<Repo> Last10Repos { get; private set; }
        private string token;

        public GithubService(UpdateService updateService)
		{
            //this.client = client;
            updateService = new UpdateService();
            this.updateService = updateService;
            Connections = new Dictionary<string, List<IDisposable>>();
            NewRepositoryResultsLogger = new List<RepositoryResult>();
            RecentLanguages = new List<LanguageResult>();
            Last10Repos = new List<Repo>();
        }


        public async Task<SearchCodeResult> GetPage(string token, string keyword, int page, string language, int perPage = 100)
		{
           
            if (client == null)
            {
                this.token = token;
                InitializeClient();
            }
          
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

        private void InitializeClient()
        {

            var productInformation = new ProductHeaderValue("keysCollector");
            var credentials = new Credentials(this.token);
            client = new GitHubClient(productInformation)
            {
                Credentials = credentials
            };
        }

        public IObservable<IEnumerable<Repo>> ObservedRepos(string keyword, int pagesCount, string language, int perPage = 100)
        {
			var pages = new IObservable<SearchCodeResult>[pagesCount];
            for (int i = 1; i <= pagesCount; i++)
            {
                async Task<SearchCodeResult> functionAsync() => await GetPage(this.token, keyword, pagesCount, language, perPage);
                pages[i - 1] = Observable.FromAsync(functionAsync);
            }

            //var notNullPagesCount = pages.Where(x => x != null).Count();
            var modelLanguage = string.Empty;

            try
            {
                return Observable.Merge(pages).Take(pagesCount).Buffer(pagesCount) //.Take(notNullPagesCount).Buffer(notNullPagesCount)//
                .Select(x => x.SelectMany(x => x == null ? new List<SearchCode>() : x.Items))//x == null ? default :
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

        public void EstablishConnections(string token, RequestModel requestModel)
        {
            if (client == null)
            {
                this.token = token;
                InitializeClient();
            }
            updateService.Add(requestModel.Keyword);

            try
            {
                var conn = Observable.Interval(TimeSpan.FromSeconds(requestModel.frequency))
                   .Subscribe(x => ObservedRepos(requestModel.Keyword, requestModel.PageNumbers, requestModel.Language)
                                   .Subscribe(x => {
                                       try
                                       {
                                           if (x != null)
                                               updateService.NotifyRepos(requestModel.Keyword, x.ToList());
                                       }
                                       catch (Exception) { }
                                   } 
               ));

                AddToDictionary(Connections, requestModel.Keyword, new List<IDisposable>(), conn);
                Connections[requestModel.Keyword].Add(
                    updateService.Repos[requestModel.Keyword].Subscribe(x => GetRecentRepos(requestModel.Keyword, x))
                );
                Connections[requestModel.Keyword].Add(
                    updateService.Repos[requestModel.Keyword].Subscribe(x => GetRecentLanguagesUsed(requestModel.Keyword))
                );
                Connections[requestModel.Keyword].Add(
                    updateService.Repos[requestModel.Keyword].Subscribe(async (x)=> await GetLast10Repos(requestModel.Keyword, x))
                );

            }
            catch (Exception) { }

        }

        public List<Repo> GetRecentRepos(string keyword, List<Repo> list)
        {

            int index = NewRepositoryResultsLogger.FindIndex(item => item.Key == keyword);
            if (index >= 0)
            {
                NewRepositoryResultsLogger[index].ResultList = list;
            }
            else
            {
                if(list.Count<1) return updateService.GetDistinctRepos(keyword, list);
                NewRepositoryResultsLogger.Add(new RepositoryResult(keyword, list));
            }

            return updateService.GetDistinctRepos(keyword, list);
        }

        public async Task<IEnumerable<Repo>> GetLast10Repos(string keyword, List<Repo> list)
        {
            if (updateService.Last10Repos.ContainsKey(keyword))
            {
                var first = await updateService.Last10Repos[keyword].FirstOrDefaultAsync();
                var last = first.Concat(list).OrderByDescending(x=>x.Date).Take(10).ToList();

                if (first.Count != last.Count()
                    ||
                    Enumerable.SequenceEqual(first.OrderBy(e => e), last.OrderBy(e => e)))
                {
                    updateService.NotifyLast10(keyword, last);
                    Last10Repos = last;
                    return last;
                }
            }
            else
            {
                var res = list.Take(10).ToList();
                updateService.NotifyLast10(keyword, res);
                Last10Repos = res;
                return res;
            }
            return default;
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

        public List<Models.Language> GetRecentLanguagesUsed(string keyword)
        {
            var res = updateService.Current[keyword].Where(x => x.Date > DateTime.Today.AddDays(-3))
                .GroupBy(x => x.LanguageName)
                .Select(x => new Models.Language(x.Key, x.Sum(sel => sel.CoincidenceIndex)))
                .OrderBy(x => x.CoincidenceIndex)
                .ToList();

            int index = RecentLanguages.FindIndex(item => item.Key == keyword);
            if (index >= 0)
            {
                RecentLanguages[index].ResultList = res;
            }
            else
            {
                RecentLanguages.Add(new LanguageResult(keyword, res));
            }

            return res;
        }


	}
}
