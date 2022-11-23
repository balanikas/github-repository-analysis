using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Security;

public class SecurityAnalyzer
{
    private readonly AnalysisContext _context;

    public SecurityAnalyzer(
        AnalysisContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Rule>> Analyze()
    {
        var rules = new List<Rule>();
        return await Task.FromResult(rules);
    }
}