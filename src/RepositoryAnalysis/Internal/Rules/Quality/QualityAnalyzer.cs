using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Quality;

internal class QualityAnalyzer : AnalyzerBase
{
    public QualityAnalyzer(
        RulesRepository repository,
        ILogger<QualityAnalyzer> logger) : base(repository, logger, RuleCategory.Quality)
    {
    }
}