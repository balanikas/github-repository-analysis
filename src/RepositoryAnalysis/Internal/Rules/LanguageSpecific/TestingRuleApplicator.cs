using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.LanguageSpecific;

internal class TestingRuleApplicator : IRuleApplicator
{
    public string RuleName => "testing";
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
            var projects = context.GitTree.FilesRecursive(IsTestProject).ToArray();
            var files = context.GitTree.FilesRecursive(IsTestFile).ToArray();
            var details = $@"
Detected {projects.Length} test projects.
Detected {files.Length} test files.
";
            return !projects.Any() || !files.Any()
                ? new(Diagnosis.Warning, "found issues", details)
                : new RuleDiagnostics(Diagnosis.Info, "found", details);
        }

        bool IsTestProject(
            GitTree.Node x) =>
            x.PathEndsWith(".csproj") && x.ParentPathEndsWith("test", "tests", "spec", "specs");

        bool IsTestFile(
            GitTree.Node x)
        {
            return x.PathEndsWith(".cs") switch
            {
                true when x.PathEndsWith("test.cs", "tests.cs", "spec.cs", "specs.cs") => true,
                _ => false
            };
        }

        return Rule.Create(this, diagnostics, new()
        {
            Text = @"
Tests increase the quality of software. 
",
            AboutLink = new("testing in dotnet", "https://learn.microsoft.com/en-us/dotnet/core/testing/")
        });
    }
}