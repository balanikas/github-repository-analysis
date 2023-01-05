using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules;

internal interface IRuleApplicator
{
    string RuleName { get; }
    RuleCategory Category { get; }
    Language Language { get; }

    Task<Rule> ApplyAsync(AnalysisContext context);
}