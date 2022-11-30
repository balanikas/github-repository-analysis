using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

public static class ILoggerExtensions
{
    public static void LogRules<T>(
        this ILogger<T> logger,
        IEnumerable<Rule> rules)
    {
        foreach (var rule in rules)
        {
            logger.LogInformation("Rule {Rule} applied with diagnosis {Diagnosis}", rule.Name, rule.Diagnosis);
        }
    }
}