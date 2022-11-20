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
        rules.AddRange(GetLanguageSpecificRules(_context.Repo.PrimaryLanguage.Name));
        return await Task.FromResult(rules);
    }

    private IEnumerable<Rule> GetLanguageSpecificRules(
        string language)
    {
        return language switch
        {
            "C#" => new[] { new Rule() },
            _ => Array.Empty<Rule>()
        };
    }
}