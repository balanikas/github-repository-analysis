using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class CommunityAnalyzer : AnalyzerBase
{
    public CommunityAnalyzer(
        RulesRepository repository,
        ILogger<CommunityAnalyzer> logger) : base(repository, logger, RuleCategory.Community)
    {
    }
}