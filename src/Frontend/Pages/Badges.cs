using MudBlazor;
using RepositoryAnalysis.Model;

namespace Frontend.Pages;

public record struct Badges
{
    public Badge DocumentationBadge { get; private init; }
    public Badge QualityBadge { get; private init; }
    public Badge CommunityBadge { get; private init; }
    public Badge SecurityBadge { get; private init; }
    public Badge LanguageSpecificBadge { get; private init; }

    public static Badges GetBadges(
        RepoAnalysis analysis) =>
        new()
        {
            DocumentationBadge = GetBadge(analysis.Documentation),
            QualityBadge = GetBadge(analysis.Quality),
            CommunityBadge = GetBadge(analysis.Community),
            SecurityBadge = GetBadge(analysis.Security),
            LanguageSpecificBadge = GetBadge(analysis.LanguageSpecific)
        };

    private static Badge GetBadge(
        IReadOnlyList<Rule> rules)
    {
        var errors = rules.Count(x => x.Diagnosis == Diagnosis.Error);
        if (errors > 0)
            return new Badge
            {
                Count = errors.ToString(),
                Color = Color.Warning
            };
        
        var warnings = rules.Count(x => x.Diagnosis == Diagnosis.Warning);
        if (warnings > 0)
            return new Badge
            {
                Count = warnings.ToString(),
                Color = Color.Info
            };
        
        return new Badge
        {
            Count = "",
            Color = Color.Transparent
        };
    }

    public record struct Badge
    {
        public required string Count { get; init; }
        public required Color Color { get; init; }
    }
}