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

    public OverView? OverView { get; init; }
    public IReadOnlyList<Rule> Rules { get; init; } = Array.Empty<Rule>();
    public AnalysisStatus Status { get; init; } = AnalysisStatus.Ok;
    public DateTime UpdatedAt { get; init; }
    public DateTime PushedAt { get; init; }

    public IReadOnlyList<string> Issues { get; init; }
}