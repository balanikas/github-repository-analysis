namespace RepositoryAnalysis;

public static class Shared
{
    public static string? GetEntryUrl(
        AnalysisContext context,
        GitHubApi.Entry? entry)
    {
        return entry is not null
            ? Path.Combine(context.Repo.Url, entry.Type, context.Repo.DefaultBranchRef.Name, entry.Path)
            : null;
    }

    public static string? CreateIssueTemplateLink(
        AnalysisContext context,
        string path,
        string name)
    {
        var url = Path.Combine(context.Repo.Url, "blob", context.Repo.DefaultBranchRef.Name, ".github/ISSUE_TEMPLATE", path);
        return $@"<strong><a target=""_blank"" href=""{url}"">{name}</a></strong>";
    }

    public static string? CreateLink(
        string url,
        string name)
    {
        return $@"<u><strong><a target=""_blank"" href=""{url}"">{name}</a></strong></u>";
    }

    public static GitHubApi.Entry? GetBlob(
        IReadOnlyList<GitHubApi.Entry>? entries,
        Func<GitHubApi.Entry, bool> predicate)
    {
        return entries?.SingleOrDefault(x =>
            x.Type.Equals("blob", StringComparison.OrdinalIgnoreCase) &&
            predicate(x));
    }

    public static GitHubApi.Entry? GetBlobRecursive(
        IReadOnlyList<GitHubApi.Entry>? entries,
        Func<GitHubApi.Entry, bool> predicate)
    {
        if (entries is null)
            return null;

        var entry = entries.SingleOrDefault(x =>
            x.Type.Equals("blob", StringComparison.OrdinalIgnoreCase) &&
            predicate(x));

        if (entry is not null) return entry;

        foreach (var e in entries.Where(x => x.Type.Equals("tree", StringComparison.OrdinalIgnoreCase)))
        {
            var found = GetBlobRecursive(e.Object?.Entries, predicate);
            if (found is not null) return found;
        }

        return null;
    }

    public static IEnumerable<GitHubApi.Entry> GetBlobsRecursive(
        IReadOnlyList<GitHubApi.Entry>? entries,
        Func<GitHubApi.Entry, bool> predicate)
    {
        if (entries is null)
            return new List<GitHubApi.Entry>();

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
        IReadOnlyList<GitHubApi.Entry>? entries,
        Func<GitHubApi.Entry, bool> predicate,
        Action<GitHubApi.Entry, IReadOnlyList<GitHubApi.Entry>> action)
    {
        if (entries is null)
            return;

        var foundEntry = entries.FirstOrDefault(x => x.Type.Equals("blob", StringComparison.OrdinalIgnoreCase) &&
                                                     predicate(x));
        if (foundEntry is not null) action(foundEntry, entries);

        foreach (var e in entries.Where(x => x.Type.Equals("tree", StringComparison.OrdinalIgnoreCase))) AnalyzeRecursive(e.Object?.Entries, predicate, action);
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