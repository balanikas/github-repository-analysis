using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

public static class ILoggerExtensions
{
    public static void LogRules<T>(
        this ILogger<T> logger,
        IEnumerable<Rule> rules,
        string url)
    {
        foreach (var rule in rules) logger.LogInformation("Rule {Rule} applied with diagnosis {Diagnosis} for {Url}", rule.Name, rule.Diagnosis, url);
    }
}