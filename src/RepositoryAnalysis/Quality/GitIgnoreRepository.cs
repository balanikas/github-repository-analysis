using System.Text;
using Octokit;

namespace RepositoryAnalysis.Quality;

public static class GitIgnoreRepository
{
    private static readonly IDictionary<string, string[]> Templates = new Dictionary<string, string[]>();

    public static async Task<string[]> GetDiff(
        IGitHubClient client,
        string language,
        string content)
    {
        var key = GetGitIgnoreKey(language);
        if (!Templates.ContainsKey(key))
        {
            var template = await client.GitIgnore.GetGitIgnoreTemplate(key);
            var templateLines = template.Source.Split("\n")
                .Where(x => !x.StartsWith("#") && !string.IsNullOrWhiteSpace(x)).ToArray();
            Templates[key] = templateLines;
        }

        var gitIgnore = Templates[key];
        var data = Convert.FromBase64String(content);
        var decodedString = Encoding.UTF8.GetString(data);
        var gitIgnoreRepo = decodedString.Split("\n")
            .Where(x => !x.StartsWith("#") && !string.IsNullOrWhiteSpace(x)).ToArray();

        return gitIgnore.Except(gitIgnoreRepo).ToArray();
    }

    public static async Task<GitIgnoreTemplate> GetTemplate(
        IGitHubClient client,
        string language)
    {
        var key = GetGitIgnoreKey(language);

        return await client.GitIgnore.GetGitIgnoreTemplate(key);
    }

    private static string GetGitIgnoreKey(
        string language)
    {
        return language switch
        {
            "C#" => "VisualStudio",
            _ => language
        };
    }
}