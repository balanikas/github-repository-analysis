using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules;

public abstract class AnalyzerBase : IAnalyzer
{
    protected readonly ILogger Logger;
    protected readonly RulesRepository Repository;

    protected AnalyzerBase(
        RulesRepository repository,
        ILogger logger,
        RuleCategory category)
    {
        Repository = repository;
        Logger = logger;
        Category = category;
    }

    public RuleCategory Category { get; }

    public virtual async Task<IReadOnlyList<Rule>> Analyze(
        AnalysisContext context)
    {
        var appliedRules = new List<Rule>();
        foreach (var r in Repository.GetRulesByCategory(Category))
        {
            var t = await Logger.LogPerfAsync(() => r.ApplyAsync(context));
            appliedRules.Add(t);
        }

        return appliedRules;
    }
}