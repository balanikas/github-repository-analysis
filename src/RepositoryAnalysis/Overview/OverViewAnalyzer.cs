using RepositoryAnalysis.Model;

namespace RepositoryAnalysis;

public class OverViewAnalyzer
{
    private readonly AnalysisContext _context;

    public OverViewAnalyzer(
        AnalysisContext context)
    {
        _context = context;
    }

    public async Task<OverView> Analyze()
    {
        var overview = new OverView
        {
            Description = _context.Repo.Description,
            Url = _context.Repo.Url,
            License = _context.Repo.LicenseInfo?.Name,
            LicenseUrl = _context.Repo.LicenseInfo?.Url,
            ImageUrl = _context.Repo.OpenGraphImageUrl,
            PrimaryLanguage = _context.Repo.PrimaryLanguage.Name,
            HomePageUrl = _context.Repo.HomepageUrl,
            DiskUsage = _context.Repo.DiskUsage,
            LastUpdated = Shared.TimeAgo(_context.Repo.UpdatedAt)
        };

        return await Task.FromResult(overview);
    }
}