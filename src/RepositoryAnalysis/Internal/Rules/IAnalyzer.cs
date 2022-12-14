using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules;

public interface IAnalyzer
{
    public RuleCategory Category { get; }

    Task<IReadOnlyList<Rule>> Analyze(
        AnalysisContext context);
}