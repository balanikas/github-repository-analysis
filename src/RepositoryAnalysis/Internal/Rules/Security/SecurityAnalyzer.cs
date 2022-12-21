using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Security;

internal class SecurityAnalyzer : AnalyzerBase
{
    public SecurityAnalyzer(
        RulesRepository repository,
        ILogger<SecurityAnalyzer> logger) : base(repository, logger, RuleCategory.Security)
    {
    }
}