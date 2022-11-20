namespace RepositoryAnalysis.Model;

public record OverView
{
    public string? ImageUrl { get; init; }
    public string? Description { get; init; }
    public string? PrimaryLanguage { get; init; }
    public string? Url { get; init; }
    public string? License { get; init; }
    public string? LicenseUrl { get; init; }
    public string? HomePageUrl { get; init; }
    public int DiskUsage { get; init; }
    public string? LastUpdated { get; init; }
}