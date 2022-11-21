namespace RepositoryAnalysis.Model;

public record RepoAnalysis
{
    public enum AnalysisStatus
    {
        Ok,
        NotFound,
        Error
    }

    public static RepoAnalysis Empty => new();
    public static RepoAnalysis Error => new() { Status = AnalysisStatus.Error };
    public static RepoAnalysis NotFound => new() { Status = AnalysisStatus.NotFound };

    public OverView? OverView { get; init; } //= new();
    public IReadOnlyList<Rule> Quality { get; init; } = Array.Empty<Rule>();
    public IReadOnlyList<Rule> Documentation { get; init; } = Array.Empty<Rule>();
    public IReadOnlyList<Rule> Community { get; init; } = Array.Empty<Rule>();
    public IReadOnlyList<Rule> Security { get; init; } = Array.Empty<Rule>();
    public IReadOnlyList<Rule> LanguageSpecific { get; init; } = Array.Empty<Rule>();
    public AnalysisStatus Status { get; init; } = AnalysisStatus.Ok;
}