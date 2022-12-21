using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Documentation;

internal class DocumentationAnalyzer : AnalyzerBase
{
    public DocumentationAnalyzer(
        RulesRepository repository,
        ILogger<DocumentationAnalyzer> logger) : base(repository, logger, RuleCategory.Documentation)
    {
    }
}