using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules;

public interface IRuleApplicator
{
    string RuleName { get; }
    RuleCategory Category { get; }
    Language Language { get; }

    Task<Rule> ApplyAsync(
        AnalysisContext context);
}