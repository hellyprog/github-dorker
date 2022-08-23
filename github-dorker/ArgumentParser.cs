namespace GithubDorker
{
    public class ArgumentParser
    {
        public Dictionary<string, List<string>> ParseArguments(string[] arguments)
        {
            var result = new Dictionary<string, List<string>>();

            foreach (var item in arguments)
            {
                if (item.StartsWith('-') || item.StartsWith("--"))
                {
                    result.Add(item, new List<string>());
                }
                else if (result.Keys.Any())
                {
                    result.Last().Value.Add(item);
                }
            }

            return result;
        }
    }
}
