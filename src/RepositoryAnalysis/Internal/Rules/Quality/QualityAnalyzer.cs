using Microsoft.Extensions.Logging;

namespace RepositoryAnalysis.Internal.Rules.Quality;

public class QualityAnalyzer : AnalyzerBase
{
    public QualityAnalyzer(
        RulesRepository repository,
        ILogger<QualityAnalyzer> logger) : base(repository, logger, RuleCategory.Quality)
    {
    }
}