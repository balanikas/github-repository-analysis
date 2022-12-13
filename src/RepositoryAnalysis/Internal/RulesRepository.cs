using RepositoryAnalysis.Internal.Rules;

namespace RepositoryAnalysis.Internal;

public class RulesRepository
{
    private readonly IReadOnlyList<IRuleApplicator> _rules;

    public RulesRepository(
        IEnumerable<IRuleApplicator> rules) => _rules = rules.ToArray();

    public IReadOnlyList<IRuleApplicator> GetRulesByCategory(
        RuleCategory category) =>
        _rules.Where(x => x.Category == category).ToArray();

    public IReadOnlyList<IRuleApplicator> GetRulesByCategoryAndLanguage(
        RuleCategory category,
        Language language) =>
        _rules.Where(x => x.Category == category && x.Language == language).ToArray();
}