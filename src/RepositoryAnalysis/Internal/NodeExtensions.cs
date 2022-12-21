using Octokit;

namespace RepositoryAnalysis.Internal;

public static class NodeExtensions
{
    public static string? GetUrl(
        this GitTree.Node? node,
        AnalysisContext context) =>
        node is not null
            ? Path.Combine(context.Repo.Url.ToString(), node.Item.Type.ToString(), context.Repo.DefaultBranchRef!.Name, node.Item.Path)
            : null;

    public static bool HasExtension(
        this GitTree.Node node,
        params string[] values) =>
        values.Any(value => Path.GetExtension(node.Item.Path).Equals(value, StringComparison.OrdinalIgnoreCase));

    public static bool HasFileName(
        this GitTree.Node node,
        params string[] values) =>
        values.Any(value => Path.GetFileName(node.Item.Path).Equals(value, StringComparison.OrdinalIgnoreCase));

    public static bool PathContains(
        this GitTree.Node node,
        params string[] values) =>
        values.Any(value => node.Item.Path.Contains(value, StringComparison.OrdinalIgnoreCase));

    public static bool PathEquals(
        this GitTree.Node node,
        params string[] values) =>
        values.Any(value => node.Item.Path.Equals(value, StringComparison.OrdinalIgnoreCase));


    public static bool PathEndsWith(
        this GitTree.Node node,
        params string[] values) =>
        values.Any(value => node.Item.Path.EndsWith(value, StringComparison.OrdinalIgnoreCase));

    public static bool ParentPathEndsWith(
        this GitTree.Node node,
        params string[] values)
    {
        var parent = Path.GetDirectoryName(node.Item.Path);
        return values.Any(value => parent != null && parent.EndsWith(value, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsTree(
        this GitTree.Node node) =>
        node.Item.Type == TreeType.Tree;
}