using Octokit;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

internal static class NodeExtensions
{
    public static string? GetUrl(
        this GitTree.Node? node,
        AnalysisContext context) =>
        node is not null
            ? Path.Combine(context.Repo.Url.ToString(), node.Item.Type.ToString(), context.Repo.DefaultBranchRef!.Name, node.Item.Path)
            : null;

    public static Link? GetLink(
        this GitTree.Node? node,
        AnalysisContext context) =>
        node is not null
            ? new(node.Item.Path, Path.Combine(context.Repo.Url.ToString(), node.Item.Type.ToString(), context.Repo.DefaultBranchRef!.Name, node.Item.Path))
            : null;

    public static string GetEmbeddedLink(
        this GitTree.Node? node,
        AnalysisContext context) =>
        node is not null
            ? Shared.CreateEmbeddedLink(
                Path.Combine(context.Repo.Url.ToString(), node.Item.Type.ToString(), context.Repo.DefaultBranchRef!.Name, node.Item.Path),
                node.Item.Path)
            : "";

    public static IReadOnlyList<string> GetEmbeddedLinks(
        this IReadOnlyList<GitTree.Node>? nodes,
        AnalysisContext context) =>
        nodes is not null
            ? nodes.Select(x => x.GetEmbeddedLink(context)).ToList()
            : Array.Empty<string>();

    public static string GetEmbeddedLinksAsString(
        this IReadOnlyList<GitTree.Node>? nodes,
        AnalysisContext context) =>
        string.Join("<br/>", nodes.GetEmbeddedLinks(context));

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

    public static bool IsTree(this GitTree.Node node) =>
        node.Item.Type == TreeType.Tree;
}