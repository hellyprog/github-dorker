using Newtonsoft.Json;
using System.IO;
using System.Web;

namespace GithubDorker
{
    class GithubDorker
    {
        const int batchSize = 14;

        static async Task Main(string[] args)
        {
            DisplayLogo();

            var argumentParser = new ArgumentParser();
            var parsedArgs = argumentParser.ParseArguments(args);

            parsedArgs.TryGetValue("-df", out var files);
            var dorkFilePath = files?.FirstOrDefault() ?? throw new ArgumentException("Dork file is not provided");
            var dorkList = await GetSearchTermsAsync(dorkFilePath);

            parsedArgs.TryGetValue("-t", out var tokens);
            var parsedToken = tokens?.FirstOrDefault() ?? throw new ArgumentException("Token is not provided");

            parsedArgs.TryGetValue("-org", out var organizations);
            var parsedOrganization = organizations?.First() ?? string.Empty;

            var httpClient = CreateGithubClient(parsedToken);
            var links = new List<string>();

            for (int i = 0; i < Math.Ceiling(dorkList.Count / (double)batchSize); i++)
            {
                var currentBatch = dorkList.Skip(i * batchSize)
                    .Take(batchSize)
                    .Select(x => httpClient.GetAsync($"/search/code?q={HttpUtility.UrlEncode(x)}+org:{parsedOrganization}")
                        .ContinueWith(httpCallTask => new
                        {
                            Response = httpCallTask.Result,
                            SearchItem = x
                        }))
                    .ToList();

                var data = await Task.WhenAll(currentBatch);

                foreach (var item in data)
                {
                    if (item.Response.IsSuccessStatusCode)
                    {
                        var link = await ProcessSuccessfullResponseAsync(item.Response, item.SearchItem, parsedOrganization).ConfigureAwait(true);

                        if (!string.IsNullOrEmpty(link))
                        {
                            links.Add(link);
                        }
                    } 
                }

                Console.WriteLine("Sleep for a minute to avoid rate-limitting");
                await Task.Delay(60 * 1000);
            }

            await WriteResultToFileAsync(parsedOrganization, links);
        }

        private static void DisplayLogo()
        {
            Console.WriteLine(@"
                |||          _ _   _           _               _            _              |||
                |||     __ _(_) |_| |__  _   _| |__         __| | ___  _ __| | _____ _ __  |||
                |||    / _` | | __| '_ \| | | | '_ \ _____ / _` |/ _ \| '__| |/ / _ \ '__| |||
                |||   | (_| | | |_| | | | |_| | |_) |_____| (_| | (_) | |  |   <  __/ |    |||
                |||    \__, |_|\__|_| |_|\__,_|_.__/       \__,_|\___/|_|  |_|\_\___|_|    |||
                |||    |___/                                                               |||
            ");
        }

        static async Task WriteResultToFileAsync(string parsedOrganization, List<string> links)
        {
            var directoryExists = Directory.Exists(@"C://github-dorker");

            if (!directoryExists)
            {
                Directory.CreateDirectory(@"C://github-dorker");
            }

            await File.WriteAllTextAsync($@"C://github-dorker/{parsedOrganization}-{DateTime.Now}.txt", links.Aggregate((a, b) => a + b + '\n'));
        }

        static async Task<string> ProcessSuccessfullResponseAsync(HttpResponseMessage response, string currentItem, string organizationName)
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GithubResponse>(content);
            
            Console.WriteLine($"Searching: {currentItem} ({result?.total_count})");
            Console.WriteLine(result?.total_count > 0
                ? $"https://github.com/search?q=org%3A{organizationName}+{currentItem}"
                : "No result found for this term");

            Console.WriteLine();

            return result?.total_count > 0 ? $"https://github.com/search?q=org%3A{organizationName}+{currentItem}" : default;
        }

        static HttpClient CreateGithubClient(string apiToken)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://api.github.com");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"token {apiToken}");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "github-dorker");
            httpClient.Timeout = TimeSpan.FromMinutes(1);

            return httpClient;
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