using RepositoryAnalysis.Internal.Rules;

namespace RepositoryAnalysis.Internal;

internal static class Shared
{
    public static Language ParseLanguage(
        string? language)
    {
        if (Enum.TryParse<Language>(language, true, out var lang)) return lang;

        //fallback
        return language?.ToLower() switch
        {
            "c#" => Language.CSharp,
            _ => Language.None
        };
    }

    public static string CreateIssueTemplateLink(
        AnalysisContext context,
        string path,
        string name)
    {
        var url = Path.Combine(context.Repo.Url.ToString(), "blob", context.Repo.DefaultBranchRef?.Name!, ".github/ISSUE_TEMPLATE", path);
        return $@"<strong><a target=""_blank"" href=""{url}"">{name}</a></strong>";
    }

    public static string CreateLink(
        string url,
        string name) =>
        $@"<u><strong><a target=""_blank"" href=""{url}"">{name}</a></strong></u>";

    public static string TimeAgo(
        DateTime? dateTime)
    {
        if (dateTime is null) return "";

        string result;
        var timeSpan = DateTime.Now.Subtract(dateTime.Value);

        if (timeSpan <= TimeSpan.FromHours(24))
            result = timeSpan.Hours > 1
                ? $"about {timeSpan.Hours} hours ago"
                : "about an hour ago";
        else if (timeSpan <= TimeSpan.FromDays(30))
            result = timeSpan.Days > 1
                ? $"about {timeSpan.Days} days ago"
                : "yesterday";
        else if (timeSpan <= TimeSpan.FromDays(365))
            result = timeSpan.Days > 30
                ? $"about {timeSpan.Days / 30} months ago"
                : "about a month ago";
        else
            result = timeSpan.Days > 365
                ? $"about {timeSpan.Days / 365} years ago"
                : "about a year ago";

        return result;
    }
}