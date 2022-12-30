using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules;

internal abstract class AnalyzerBase : IAnalyzer
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
        AnalysisContext context) =>
        await Analyze(context, Repository.GetRulesByCategory(Category));

    protected async Task<IReadOnlyList<Rule>> Analyze(
        AnalysisContext context,
        IReadOnlyList<IRuleApplicator> applicators)
    {
        var appliedRules = new List<Rule>();
        foreach (var ruleApplicator in applicators)
            try
            {
                var _ = Stopwatch.StartNew();
                var rule = await ruleApplicator.ApplyAsync(context);
                _.Stop();
                Logger.LogInformation("{Name} took {Elapsed} ms", rule.Name, _.ElapsedMilliseconds);
                appliedRules.Add(rule);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to apply rule {Name}", ruleApplicator.RuleName);

                appliedRules.Add(Rule.Create(ruleApplicator, RuleDiagnostics.CreateFailed(), new()
                {
                    Text = "failed to apply this rule"
                }));
            }

        return appliedRules;
    }
}