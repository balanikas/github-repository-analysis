using Microsoft.Extensions.Logging;

namespace RepositoryAnalysis.Internal.Rules.Community;

public class CommunityAnalyzer : AnalyzerBase
{
    public CommunityAnalyzer(
        RulesRepository repository,
        ILogger<CommunityAnalyzer> logger) : base(repository, logger, RuleCategory.Community)
    {
    }
}