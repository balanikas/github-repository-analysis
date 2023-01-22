using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.LanguageSpecific;

internal class TestingRuleApplicator : IRuleApplicator
{
    [RuleGuidance(200, Tone.Motivational, Complexity.Simple)]
    private const string WhatIs = "Why is it important to have a well tested .net solution?";

    [RuleGuidance] private const string TestTypes = "What types of testing is typically involved in a .net solution?";
    [RuleGuidance] private const string CommonFrameworks = "List some common testing frameworks for .net solutions.";

    private readonly IGpt3Client _gpt3Client;

    public TestingRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "testing";
    public RuleCategory Category => RuleCategory.LanguageSpecific;
    public Language Language => Language.CSharp;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
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
                ? new RuleDiagnostics(Diagnosis.Warning, "found issues", details)
                : new RuleDiagnostics(Diagnosis.Info, "found", details);
        }

        bool IsTestProject(GitTree.Node x) =>
            x.PathEndsWith(".csproj") && x.ParentPathEndsWith("test", "tests", "spec", "specs");

        bool IsTestFile(GitTree.Node x)
        {
            return x.PathEndsWith(".cs") switch
            {
                true when x.PathEndsWith("test.cs", "tests.cs", "spec.cs", "specs.cs") => true,
                _ => false
            };
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(TestTypes, CommonFrameworks),
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("testing in dotnet", "https://learn.microsoft.com/en-us/dotnet/core/testing/")
        });
    }
}