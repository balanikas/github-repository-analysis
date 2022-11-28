using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

public interface IAnalyzer
{
    Task<IReadOnlyList<Rule>> Analyze(
        AnalysisContext context);
}