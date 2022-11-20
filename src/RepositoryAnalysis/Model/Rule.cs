namespace RepositoryAnalysis.Model;

public record Rule
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; init; } = "";
    public string Note { get; init; } = "";
    public string? ResourceName { get; init; }
    public string? ResourceUrl { get; init; }
    public Explanation Explanation { get; init; } = new();
    public Diagnosis Diagnosis { get; init; }
    public bool ShowDetails { get; set; }
}