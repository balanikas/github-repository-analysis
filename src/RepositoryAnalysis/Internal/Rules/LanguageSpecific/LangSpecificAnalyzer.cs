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

    public override async Task<IReadOnlyList<Rule>> Analyze(AnalysisContext context)
    {
        var parsedLang = Shared.ParseLanguage(context.Repo.PrimaryLanguage?.Name);
        if (parsedLang is Language.None) return new List<Rule>();

        return await Analyze(context, Repository.GetRulesByCategoryAndLanguage(Category, parsedLang));
    }
}