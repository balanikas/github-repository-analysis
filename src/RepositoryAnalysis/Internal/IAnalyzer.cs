using RepositoryAnalysis.Internal.Rules;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

public interface IAnalyzer
{
    public RuleCategory Category { get; }

    Task<IReadOnlyList<Rule>> Analyze(
        AnalysisContext context);
}