using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

public static class ILoggerExtensions
{
    public static Rule LogPerf<T>(
        this ILogger<T> logger,
        Func<Rule> f)
    {
        var _ = Stopwatch.StartNew();
        var result = f();
        _.Stop();
        logger.LogInformation("{Name} took {Elapsed} ms", result.Name, _.ElapsedMilliseconds);
        return result;
    }
    
    public static async Task<Rule> LogPerfAsync<T>(
        this ILogger<T> logger,
        Func<Task<Rule>> f)
    {
        var _ = Stopwatch.StartNew();
        var result = await f();
        _.Stop();
        logger.LogInformation("{Name} took {Elapsed} ms", result.Name, _.ElapsedMilliseconds);
        return result;
    }
    
    public static void LogRules<T>(
        this ILogger<T> logger,
        IEnumerable<Rule> rules,
        string url)
    {
        var logs = rules.Select(rule => new RuleLog { Name = rule.Name, Diagnosis = rule.Diagnosis });
        logger.LogInformation("Analysis of rules {Rules} for {Url}", logs, url);
    }

    private record RuleLog
    {
        public Diagnosis Diagnosis;
        public string Name;
    }
}