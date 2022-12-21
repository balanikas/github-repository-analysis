using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.LanguageSpecific;

internal class LangSpecificAnalyzer : AnalyzerBase
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
        var parsedLang = Shared.ParseLanguage(context.Repo.PrimaryLanguage?.Name);
        if (parsedLang is Language.None) return appliedRules;
        foreach (var r in Repository.GetRulesByCategoryAndLanguage(Category, parsedLang))
        {
            var t = await Logger.LogPerfAsync(() => r.ApplyAsync(context));
            appliedRules.Add(t);
        }

        return appliedRules;
    }
}