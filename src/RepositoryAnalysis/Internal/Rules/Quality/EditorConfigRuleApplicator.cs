using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Quality;

public class EditorConfigRuleApplicator : IRuleApplicator
{
    public string RuleName => "editorconfig";
    public RuleCategory Category => RuleCategory.Quality;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var node = context.GitTree.FirstFileOrDefaultRecursive(x => x.PathEquals(".editorconfig"));
        var (diagnosis, note) = GetDiagnosis(node);
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
            ResourceName = node?.Item.Path, ResourceUrl = node.GetUrl(context)
        };

        (Diagnosis, string) GetDiagnosis(
            GitTree.Node? e) =>
            e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Error, "missing");
    }
}