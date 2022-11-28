using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

public class SecurityAnalyzer : IAnalyzer
{
    public async Task<IReadOnlyList<Rule>> Analyze(
        AnalysisContext context)
    {
        var rules = new List<Rule>();
        return await Task.FromResult(rules);
    }
}