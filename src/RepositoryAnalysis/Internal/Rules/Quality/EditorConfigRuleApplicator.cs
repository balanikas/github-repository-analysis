using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Quality;

internal class EditorConfigRuleApplicator : IRuleApplicator
{
    public string RuleName => "editorconfig";
    public RuleCategory Category => RuleCategory.Quality;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var root = context.GitTree.FirstFileOrDefault(x => x.PathEquals(".editorconfig"));
        var nonRoot = context.GitTree.FilesRecursive(x => x.PathEquals(".editorconfig"));
        var (diagnosis, node, note) = GetDiagnosis(root, nonRoot);
        return new Rule
        {
            Name = RuleName,
            Category = Category,
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = null,
                Text = @"
EditorConfig helps maintain consistent coding styles for multiple developers working on the same project across various editors and IDEs. 
The EditorConfig project consists of a file format for defining coding styles and a collection of text editor plugins that enable editors to read the file format and adhere to defined styles. 
EditorConfig files are easily readable and they work nicely with version control systems.",
                AboutUrl = "https://editorconfig.org/",
                AboutHeader = "about editor config"
            },
            ResourceName = node?.Item.Path,
            ResourceUrl = node.GetUrl(context)
        };

        (Diagnosis, GitTree.Node?, string) GetDiagnosis(
            GitTree.Node? rootFile,
            IReadOnlyList<GitTree.Node> nonRootFiles)
        {
            if (rootFile is not null)
                return nonRootFiles.Count > 1
                    ? (Diagnosis.Info, rootFile, $"found at root and {nonRootFiles.Count} files at other locations")
                    : (Diagnosis.Info, rootFile, "found at root");

            return nonRootFiles.Count > 0
                ? (Diagnosis.Info, nonRootFiles.First(), $"missing at root but found {nonRootFiles.Count} files at other locations")
                : (Diagnosis.Error, null, "missing");
        }
    }
}