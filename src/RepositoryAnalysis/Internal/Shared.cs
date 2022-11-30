namespace RepositoryAnalysis.Internal;

public static class Shared
{
    public static string? GetEntryUrl(
        AnalysisContext context,
        GitHubGraphQlClient.Entry? entry) =>
        entry is not null
            ? Path.Combine(context.Repo.Url, entry.Type, context.Repo.DefaultBranchRef.Name, entry.Path)
            : null;

    public static string CreateIssueTemplateLink(
        AnalysisContext context,
        string path,
        string name)
    {
        var url = Path.Combine(context.Repo.Url, "blob", context.Repo.DefaultBranchRef.Name, ".github/ISSUE_TEMPLATE", path);
        return $@"<strong><a target=""_blank"" href=""{url}"">{name}</a></strong>";
    }

    public static string CreateLink(
        string url,
        string name) =>
        $@"<u><strong><a target=""_blank"" href=""{url}"">{name}</a></strong></u>";

    public static GitHubGraphQlClient.Entry? GetSingleBlob(
        IReadOnlyList<GitHubGraphQlClient.Entry>? entries,
        Func<GitHubGraphQlClient.Entry, bool> predicate)
    {
        return entries?.SingleOrDefault(x =>
            x.Type.Equals("blob", StringComparison.OrdinalIgnoreCase) &&
            predicate(x));
    }

    public static GitHubGraphQlClient.Entry? GetFirstBlob(
        IReadOnlyList<GitHubGraphQlClient.Entry>? entries,
        Func<GitHubGraphQlClient.Entry, bool> predicate)
    {
        return entries?.FirstOrDefault(x =>
            x.Type.Equals("blob", StringComparison.OrdinalIgnoreCase) &&
            predicate(x));
    }

    public static GitHubGraphQlClient.Entry? GetSingleBlobRecursive(
        IReadOnlyList<GitHubGraphQlClient.Entry>? entries,
        Func<GitHubGraphQlClient.Entry, bool> predicate)
    {
        if (entries is null)
            return null;

        var entry = entries.SingleOrDefault(x =>
            x.Type.Equals("blob", StringComparison.OrdinalIgnoreCase) &&
            predicate(x));

        if (entry is not null) return entry;

        return entries
            .Where(x => x.Type.Equals("tree", StringComparison.OrdinalIgnoreCase))
            .Select(e => GetSingleBlobRecursive(e.Object?.Entries, predicate))
            .FirstOrDefault(found => found is not null);
    }

    public static GitHubGraphQlClient.Entry? GetFirstBlobRecursive(
        IReadOnlyList<GitHubGraphQlClient.Entry>? entries,
        Func<GitHubGraphQlClient.Entry, bool> predicate)
    {
        if (entries is null)
            return null;

        var entry = entries.FirstOrDefault(x =>
            x.Type.Equals("blob", StringComparison.OrdinalIgnoreCase) &&
            predicate(x));

        if (entry is not null) return entry;

        return entries
            .Where(x => x.Type.Equals("tree", StringComparison.OrdinalIgnoreCase))
            .Select(e => GetFirstBlobRecursive(e.Object?.Entries, predicate))
            .FirstOrDefault(found => found is not null);
    }

    public static IEnumerable<GitHubGraphQlClient.Entry> GetBlobsRecursive(
        IReadOnlyList<GitHubGraphQlClient.Entry>? entries,
        Func<GitHubGraphQlClient.Entry, bool> predicate)
    {
        if (entries is null)
            return new List<GitHubGraphQlClient.Entry>();

        var foundEntries = entries.Where(x =>
            x.Type.Equals("blob", StringComparison.OrdinalIgnoreCase) &&
            predicate(x)).ToList();

        foreach (var e in entries.Where(x => x.Type.Equals("tree", StringComparison.OrdinalIgnoreCase)))
        {
            var found = GetBlobsRecursive(e.Object?.Entries, predicate);
            foundEntries.AddRange(found);
        }

        return foundEntries;
    }

    public static void AnalyzeRecursive(
        IReadOnlyList<GitHubGraphQlClient.Entry>? entries,
        Func<GitHubGraphQlClient.Entry, bool> predicate,
        Action<GitHubGraphQlClient.Entry, IReadOnlyList<GitHubGraphQlClient.Entry>> action)
    {
        if (entries is null)
            return;

        foreach (var entry in entries)
            if (entry.Type.Equals("blob", StringComparison.OrdinalIgnoreCase) && predicate(entry))
                action(entry, entries);

        foreach (var e in entries.Where(x => x.Type.Equals("tree", StringComparison.OrdinalIgnoreCase)))
            AnalyzeRecursive(e.Object?.Entries, predicate, action);
    }


    public static string TimeAgo(
        DateTime dateTime)
    {
        string result;
        var timeSpan = DateTime.Now.Subtract(dateTime);

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