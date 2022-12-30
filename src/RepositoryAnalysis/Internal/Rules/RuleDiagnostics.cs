using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules;

public record RuleDiagnostics(
    Diagnosis Diagnosis,
    string Note,
    string? Details = null,
    Link? Resource = null)
{
    internal static RuleDiagnostics CreateFailed() => new(Diagnosis.Failed, "failed to apply this rule");
}