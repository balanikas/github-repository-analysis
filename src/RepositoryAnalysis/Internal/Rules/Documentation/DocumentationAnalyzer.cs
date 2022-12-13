using Microsoft.Extensions.Logging;

namespace RepositoryAnalysis.Internal.Rules.Documentation;

public class DocumentationAnalyzer : AnalyzerBase
{
    public DocumentationAnalyzer(
        RulesRepository repository,
        ILogger<DocumentationAnalyzer> logger) : base(repository, logger, RuleCategory.Documentation)
    {
    }
}