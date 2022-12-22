using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

internal static class ILoggerExtensions
{
    public static void LogRules<T>(
        this ILogger<T> logger,
        IEnumerable<Rule> rules)
    {
        var logs = rules.Select(rule => new RuleLog { Name = rule.Name, Diagnosis = rule.Diagnosis });
        logger.LogInformation("Analysis of rules {Rules}", logs);
    }

    private record RuleLog
    {
        public Diagnosis Diagnosis { get; set; }
        public string Name { get; set; }
    }
}