namespace RepositoryAnalysis.Internal;

public class OverView
{
    public Model.OverView Analyze(
        AnalysisContext context)
    {
        var overview = new Model.OverView
        {
            Description = context.Repo.Description,
            Url = context.Repo.Url,
            License = context.Repo.LicenseInfo?.Name,
            LicenseUrl = context.Repo.LicenseInfo?.Url,
            ImageUrl = context.Repo.OpenGraphImageUrl,
            PrimaryLanguage = context.Repo.PrimaryLanguage?.Name,
            HomePageUrl = context.Repo.HomepageUrl,
            DiskUsage = context.Repo.DiskUsage,
            GitObjectCount = context.GitTree.Count,
            LastUpdated = Shared.TimeAgo(context.Repo.UpdatedAt)
        };

        return overview;
    }
}