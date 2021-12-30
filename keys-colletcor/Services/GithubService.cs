using System;
using System.Threading.Tasks;
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
                Language = Enum.Parse<Language>(language),
                Page = page,
                PerPage = perPage
            };
            var res = await client.Search.SearchCode(searchRequest);
			return res;
		}
	}
}
