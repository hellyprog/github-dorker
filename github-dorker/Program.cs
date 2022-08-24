using Newtonsoft.Json;
using System.Web;

namespace GithubDorker
{
    class GithubDorker
    {
        const int batchSize = 30;

        static async Task Main(string[] args)
        {
            var argumentParser = new ArgumentParser();
            var parsedArgs = argumentParser.ParseArguments(args);

            parsedArgs.TryGetValue("-df", out var files);
            var dorkFilePath = files?.FirstOrDefault() ?? throw new ArgumentException("Dork file is not provided");
            var dorkList = await GetSearchTermsAsync(dorkFilePath);

            parsedArgs.TryGetValue("-t", out var tokens);
            var parsedToken = tokens?.FirstOrDefault() ?? throw new ArgumentException("Token is not provided");
            parsedArgs.TryGetValue("-org", out var organization);

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://api.github.com");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"token {parsedToken}");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "github-dorker");

            for (int i = 0; i < Math.Ceiling(dorkList.Count / (double)batchSize); i++)
            {
                var currentBatch = dorkList.Skip(i * batchSize)
                    .Take(batchSize);

                foreach (var item in currentBatch)
                {
                    Console.Write($"Searching: {item} ");

                    var response = await httpClient.GetAsync($"/search/code?q={HttpUtility.UrlEncode(item)}+org:{organization.First()}");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(content);
                        Console.WriteLine(result?.total_count);
                    }

                    Console.WriteLine();
                }

                await Task.Delay(60 * 1000);
            }

        }

        static async Task<List<string>> GetSearchTermsAsync(string dorkFilePath)
        {
            var result = File.Exists(dorkFilePath)
                ? await File.ReadAllLinesAsync(dorkFilePath)
                : Array.Empty<string>();

            return result.ToList();
        }
    }
}