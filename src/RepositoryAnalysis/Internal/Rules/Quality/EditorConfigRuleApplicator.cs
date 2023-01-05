using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Quality;

internal class EditorConfigRuleApplicator : IRuleApplicator
{
    public string RuleName => "editorconfig";
    public RuleCategory Category => RuleCategory.Quality;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            var rootFile = context.GitTree.FirstFileOrDefault(x => x.PathEquals(".editorconfig"));
            var nonRootFiles = context.GitTree.FilesRecursive(x => x.PathEquals(".editorconfig"));
            if (rootFile is not null)
                return nonRootFiles.Count > 1
                    ? new RuleDiagnostics(Diagnosis.Info, $"found at root and {nonRootFiles.Count} files at other locations", null, rootFile.GetLink(context))
                    : new RuleDiagnostics(Diagnosis.Info, "found at root", null, rootFile.GetLink(context));

            return nonRootFiles.Count > 0
                ? new RuleDiagnostics(Diagnosis.Info, $"missing at root but found {nonRootFiles.Count} files at other locations")
                : new RuleDiagnostics(Diagnosis.Error, "missing editorconfig file");
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            Text = @"
EditorConfig helps maintain consistent coding styles for multiple developers working on the same project across various editors and IDEs. 
The EditorConfig project consists of a file format for defining coding styles and a collection of text editor plugins that enable editors to read the file format and adhere to defined styles. 
EditorConfig files are easily readable and they work nicely with version control systems.",
            AboutLink = new Link("about editor config", "https://editorconfig.org/")
        });
    }
}