using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.LanguageSpecific;

public class LangSpecificAnalyzer : AnalyzerBase
{
    public LangSpecificAnalyzer(
        RulesRepository repository,
        ILogger<LangSpecificAnalyzer> logger)
        : base(repository, logger, RuleCategory.LanguageSpecific)
    {
    }

    public override async Task<IReadOnlyList<Rule>> Analyze(
        AnalysisContext context)
    {
        var appliedRules = new List<Rule>();
        if (!Enum.TryParse<Language>(context.Repo.PrimaryLanguage?.Name, out var lang)) return appliedRules;
        foreach (var r in Repository.GetRulesByCategoryAndLanguage(Category, lang))
        {
            var t = await Logger.LogPerfAsync(() => r.ApplyAsync(context));
            appliedRules.Add(t);
        }

        return appliedRules;
    }
}