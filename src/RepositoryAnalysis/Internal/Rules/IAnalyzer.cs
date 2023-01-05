using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules;

internal interface IAnalyzer
{
    public RuleCategory Category { get; }

    Task<IReadOnlyList<Rule>> Analyze(AnalysisContext context);
}