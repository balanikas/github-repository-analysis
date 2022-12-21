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
        var testProjects = context.GitTree.FilesRecursive(IsTestProject).ToArray();
        var testFiles = context.GitTree.FilesRecursive(IsTestFile).ToArray();
        var (diagnosis, note) = GetDiagnosis(testProjects, testFiles);
        var details = $@"
Detected {testProjects.Length} test projects.
Detected {testFiles.Length} test files.
";
        return new Rule
        {
            Name = RuleName,
            Category = Category,
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
Tests increase the quality of software. 
",
                AboutUrl = "https://learn.microsoft.com/en-us/dotnet/core/testing/",
                AboutHeader = "testing in dotnet"
            }
        };

        (Diagnosis, string) GetDiagnosis(
            IEnumerable<GitTree.Node> projects,
            IEnumerable<GitTree.Node> files) =>
            !projects.Any() || !files.Any()
                ? (Diagnosis.Warning, "found issues")
                : (Diagnosis.Info, "found");

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
    }
}