namespace RepositoryAnalysis.Model;

public record Explanation
{
    public required string Text { get; init; }
    public Link? AboutLink { get; init; }
    public Link? GuidanceLink { get; init; }
}