using Microsoft.Extensions.DependencyInjection;
using Octokit;
using System.IO;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace keys_collector
{
    public static class ConnectionSetting
    {
		public static void AddGithubClient(this IServiceCollection services)
		{
			string token;
			using (StreamReader reader = new StreamReader("token.txt"))
			{
				token = reader.ReadLine();
			}
			var productInformation = new ProductHeaderValue("keysCollector");
			var credentials = new Credentials(token);
            var client = new GitHubClient(productInformation)
            {
                Credentials = credentials
            };
            services.AddTransient(x => client);
		}
	}
}
	