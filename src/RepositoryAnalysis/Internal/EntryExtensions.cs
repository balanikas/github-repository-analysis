namespace RepositoryAnalysis.Internal;

public static class EntryExtensions
{
    public static bool HasExtension(
        this GitHubGraphQlClient.Entry entry,
        params string[] values)
    {
        return values.Any(value => Path.GetExtension(entry.Path).Equals(value, StringComparison.OrdinalIgnoreCase));
    }

    public static bool PathContains(
        this GitHubGraphQlClient.Entry entry,
        params string[] values)
    {
        return values.Any(value => entry.Path.Contains(value, StringComparison.OrdinalIgnoreCase));
    }


    public static bool PathEquals(
        this GitHubGraphQlClient.Entry entry,
        params string[] values)
    {
        return values.Any(value => entry.Path.Equals(value, StringComparison.OrdinalIgnoreCase));
    }


    public static bool PathEndsWith(
        this GitHubGraphQlClient.Entry entry,
        params string[] values)
    {
        return values.Any(value => entry.Path.EndsWith(value, StringComparison.OrdinalIgnoreCase));
    }

    public static bool ParentPathEndsWith(
        this GitHubGraphQlClient.Entry entry,
        params string[] values)
    {
        var parent = Path.GetDirectoryName(entry.Path);

        return values.Any(value => parent != null && parent.EndsWith(value, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsTree(
        this GitHubGraphQlClient.Entry entry)
    {
        return entry.Type.Equals("tree", StringComparison.OrdinalIgnoreCase);
    }
}