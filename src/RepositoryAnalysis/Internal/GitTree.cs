using Octokit;

namespace RepositoryAnalysis.Internal;

public class GitTree
{
    public GitTree(
        TreeResponse treeResponse)
    {
        Root = new Node(new TreeItem("", "", TreeType.Tree, 0, "", ""));
        Truncated = treeResponse.Truncated;
        Count = treeResponse.Tree.Count;
        var index = 0;
        Walk(Root, treeResponse.Tree, ref index, new Stack<string>());
    }

    public int Count { get; }
    public bool Truncated { get; }
    public Node Root { get; }

    public Node? SingleFileOrDefault(
        Func<Node, bool> predicate) =>
        Root.Children.SingleOrDefault(x => x.Item.Type == TreeType.Blob && predicate(x));

    public Node? FirstFileOrDefault(
        Func<Node, bool> predicate) =>
        Root.Children.FirstOrDefault(x => x.Item.Type == TreeType.Blob && predicate(x));

    public Node? SingleFileOrDefaultRecursive(
        Func<Node, bool> predicate) =>
        SingleFileOrDefaultRecursive(Root.Children, predicate, GetRecommendedSearchDepth());

    public Node? SingleFileOrDefaultRecursive(
        List<Node> nodes,
        Func<Node, bool> predicate,
        int searchDepth)
    {
        var node = nodes.SingleOrDefault(x => x.Item.Type == TreeType.Blob && predicate(x));
        if (node is not null) return node;

        if (searchDepth == 0) return null;

        return nodes
            .Where(x => x.Item.Type == TreeType.Tree)
            .Select(x => SingleFileOrDefaultRecursive(x.Children, predicate, searchDepth - 1))
            .SingleOrDefault(found => found is not null);
    }

    public Node? FirstFileOrDefaultRecursive(
        Func<Node, bool> predicate) =>
        FirstFileOrDefaultRecursive(Root.Children, predicate, GetRecommendedSearchDepth());

    public Node? FirstFileOrDefaultRecursive(
        List<Node> nodes,
        Func<Node, bool> predicate,
        int searchDepth)
    {
        var node = nodes.FirstOrDefault(x => x.Item.Type == TreeType.Blob && predicate(x));
        if (node is not null) return node;

        if (searchDepth == 0) return null;

        return nodes
            .Where(x => x.Item.Type == TreeType.Tree)
            .Select(x => FirstFileOrDefaultRecursive(x.Children, predicate, searchDepth - 1))
            .FirstOrDefault(found => found is not null);
    }

    public IReadOnlyList<Node> FilesRecursive(
        Func<Node, bool> predicate) =>
        FilesRecursive(Root.Children, predicate, GetRecommendedSearchDepth());

    public IReadOnlyList<Node> FilesRecursive(
        List<Node> nodes,
        Func<Node, bool> predicate,
        int searchDepth)
    {
        var foundNodes = nodes
            .Where(x => x.Item.Type == TreeType.Blob && predicate(x))
            .ToList();

        if (searchDepth == 0) return foundNodes;
        foreach (var node in nodes.Where(x => x.Item.Type == TreeType.Tree))
        {
            var found = FilesRecursive(node.Children, predicate, searchDepth - 1);
            foundNodes.AddRange(found);
        }

        return foundNodes;
    }

    public void AnalyzeRecursive(
        Func<Node, bool> predicate,
        Action<Node, IReadOnlyList<Node>> action) =>
        AnalyzeRecursive(Root.Children, predicate, action, GetRecommendedSearchDepth());

    public void AnalyzeRecursive(
        List<Node> nodes,
        Func<Node, bool> predicate,
        Action<Node, IReadOnlyList<Node>> action,
        int searchDepth)
    {
        foreach (var node in nodes.Where(x => x.Item.Type == TreeType.Blob && predicate(x))) action(node, nodes);

        if (searchDepth == 0) return;

        foreach (var e in nodes.Where(x => x.Item.Type == TreeType.Tree)) AnalyzeRecursive(e.Children, predicate, action, searchDepth - 1);
    }

    private int GetRecommendedSearchDepth() => Count > 1000 ? 5 : int.MaxValue;

    private void Walk(
        Node root,
        IReadOnlyList<TreeItem> tree,
        ref int currentIndex,
        Stack<string> folderStack)
    {
        var folder = root.Item.Path ?? "";
        folderStack.Push(folder);
        for (; currentIndex < tree.Count;)
        {
            var item = tree[currentIndex];
            if (folderStack.TryPeek(out var currentFolder))
                if (!item.Path.StartsWith(currentFolder))
                {
                    folderStack.Pop();
                    return;
                }

            currentIndex++;
            if (item.Type == TreeType.Blob)
            {
                root.Children.Add(new Node(item));
            }
            else if (item.Type == TreeType.Tree)
            {
                var node = new Node(item);
                root.Children.Add(node);
                Walk(node, tree, ref currentIndex, folderStack);
            }
        }
    }

    public class Node
    {
        public Node(
            TreeItem item) => Item = item;

        public TreeItem Item { get; }
        public List<Node> Children { get; } = new();

        public override string ToString() => Item.Path ?? "";
    }
}