using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.LanguageSpecific;

internal class SolutionStructureRuleApplicator : IRuleApplicator
{
    public string RuleName => "solution structure";
    public RuleCategory Category => RuleCategory.LanguageSpecific;
    public Language Language => Language.CSharp;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            var warnings = new List<string>();
            context.GitTree.AnalyzeRecursive(
                node => node.HasExtension(".sln"),
                (
                    node,
                    nodes) =>
                {
                    if (nodes.Any(x => x.HasExtension(".cs")))
                        warnings.Add($"detected source files next to the solution file {Shared.CreateEmbeddedLink(node.GetUrl(context)!, node.Item.Path)}");

                    if (nodes.Any(x => x.HasExtension(".csproj")))
                        warnings.Add($"detected project files next to the solution file {Shared.CreateEmbeddedLink(node.GetUrl(context)!, node.Item.Path)}");
                });

            var csFile = context.GitTree.FirstFileOrDefault(x => x.HasExtension(".cs"));
            if (csFile is not null) warnings.Add("found code file in root folder. Consider adding a /src folder to contain code files");

            var csProjFile = context.GitTree.FirstFileOrDefault(x => x.HasExtension(".csproj"));
            if (csProjFile is not null) warnings.Add("found project file in root folder. Consider adding a /src folder to contain project files");

            if (context.GitTree.Root.Children.Count < 5) warnings.Add("Root folder looks incomplete. Essential files can be added.");

            if (warnings.Any())
            {
                var details = $@"
Found {warnings.Count} issues.
<br/>
{string.Join("<br/>", warnings)}
";
                return new(Diagnosis.Warning, $"found {warnings.Count} potential solution structure issues", details);
            }

            return new(Diagnosis.Info, "did not find any issues");
        }

        return Rule.Create(this, diagnostics, new()
        {
            Text = @"
It is good practice to follow standard solution structure conventions. "
        });
    }
}