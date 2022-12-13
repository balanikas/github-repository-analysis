using Microsoft.Extensions.Logging;

namespace RepositoryAnalysis.Internal.Rules.Security;

public class SecurityAnalyzer : AnalyzerBase
{
    public SecurityAnalyzer(
        RulesRepository repository,
        ILogger<SecurityAnalyzer> logger) : base(repository, logger, RuleCategory.Security)
    {
    }
}