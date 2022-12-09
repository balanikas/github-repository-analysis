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
        SingleFileOrDefaultRecursive(Root.Children, predicate);

    public Node? SingleFileOrDefaultRecursive(
        List<Node> nodes,
        Func<Node, bool> predicate)
    {
        var node = nodes.SingleOrDefault(x => x.Item.Type == TreeType.Blob && predicate(x));
        if (node is not null) return node;

        return nodes
            .Where(x => x.Item.Type == TreeType.Tree)
            .Select(x => SingleFileOrDefaultRecursive(x.Children, predicate))
            .SingleOrDefault(found => found is not null);
    }

    public Node? FirstFileOrDefaultRecursive(
        Func<Node, bool> predicate) =>
        FirstFileOrDefaultRecursive(Root.Children, predicate);

    public Node? FirstFileOrDefaultRecursive(
        List<Node> nodes,
        Func<Node, bool> predicate)
    {
        var node = nodes.FirstOrDefault(x => x.Item.Type == TreeType.Blob && predicate(x));
        if (node is not null) return node;

        return nodes
            .Where(x => x.Item.Type == TreeType.Tree)
            .Select(x => FirstFileOrDefaultRecursive(x.Children, predicate))
            .FirstOrDefault(found => found is not null);
    }

    public IReadOnlyList<Node> FilesRecursive(
        Func<Node, bool> predicate) =>
        FilesRecursive(Root.Children, predicate);

    public IReadOnlyList<Node> FilesRecursive(
        List<Node> nodes,
        Func<Node, bool> predicate)
    {
        var foundNodes = nodes
            .Where(x => x.Item.Type == TreeType.Blob && predicate(x))
            .ToList();

        foreach (var node in nodes.Where(x => x.Item.Type == TreeType.Tree))
        {
            var found = FilesRecursive(node.Children, predicate);
            foundNodes.AddRange(found);
        }

        return foundNodes;
    }

    public void AnalyzeRecursive(
        Func<Node, bool> predicate,
        Action<Node, IReadOnlyList<Node>> action) =>
        AnalyzeRecursive(Root.Children, predicate, action);


    public void AnalyzeRecursive(
        List<Node> nodes,
        Func<Node, bool> predicate,
        Action<Node, IReadOnlyList<Node>> action)
    {
        foreach (var node in nodes.Where(x => x.Item.Type == TreeType.Blob && predicate(x))) action(node, nodes);

        foreach (var e in nodes.Where(x => x.Item.Type == TreeType.Tree)) AnalyzeRecursive(e.Children, predicate, action);
    }

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

        public override string ToString() => Item?.Path ?? "";
    }
}