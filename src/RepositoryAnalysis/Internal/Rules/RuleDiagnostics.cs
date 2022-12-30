using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules;

public record RuleDiagnostics(
    Diagnosis Diagnosis,
    string Note,
    string? Details = null,
    string? ResourceName = null,
    string? ResourceUrl = null)
{
    internal static RuleDiagnostics CreateFailed() => new(Diagnosis.Failed, "failed to apply this rule");
}