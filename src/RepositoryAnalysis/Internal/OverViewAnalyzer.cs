using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

public class OverViewAnalyzer
{
    public OverView Analyze(
        AnalysisContext context)
    {
        var overview = new OverView
        {
            Description = context.Repo.Description,
            Url = context.Repo.Url,
            License = context.Repo.LicenseInfo?.Name,
            LicenseUrl = context.Repo.LicenseInfo?.Url,
            ImageUrl = context.Repo.OpenGraphImageUrl,
            PrimaryLanguage = context.Repo.PrimaryLanguage?.Name,
            HomePageUrl = context.Repo.HomepageUrl,
            DiskUsage = context.Repo.DiskUsage,
            LastUpdated = Shared.TimeAgo(context.Repo.UpdatedAt)
        };

        return overview;
    }
}