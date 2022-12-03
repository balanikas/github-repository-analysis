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
        var logs = rules.Select(rule => new { rule.Name, rule.Diagnosis} );
        logger.LogInformation("Analysis of rules {Rules} for {Url}", logs, url);

    }
    
}