namespace RepositoryAnalysis.Model;

public record Explanation
{
    public string Text { get; init; } = "";
    public string? AboutUrl { get; init; }
    public string? AboutHeader { get; init; }
    public string? GuidanceUrl { get; init; }
    public string? GuidanceHeader { get; init; }
}