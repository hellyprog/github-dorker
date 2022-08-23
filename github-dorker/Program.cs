namespace GithubDorker
{
    class GithubDorker
    {
        static async Task Main(string[] args)
        {
            var apiKey = Environment.GetEnvironmentVariable("GH_TOKEN");
            var parameters = Environment.GetCommandLineArgs();
            var argumentParser = new ArgumentParser();
            var parsedArgs = argumentParser.ParseArguments(args);

            foreach (var item in parsedArgs)
            {
                Console.WriteLine($"{item.Key}, [{string.Join(',', item.Value)}]");
            }
        }
    }
}