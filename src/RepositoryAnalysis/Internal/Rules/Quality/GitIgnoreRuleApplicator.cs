using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Quality;

internal class GitIgnoreRuleApplicator : IRuleApplicator
{
    [RuleGuidance(200, Tone.Motivational, Complexity.Simple)]
    private const string WhereToFind = "Where can i find a good gitignore file for my repository?";

    [RuleGuidance] private const string MultipleFiles = "When should i have multiple gitignore files in my repository?";
    [RuleGuidance] private const string WhatIs = "What is gitignore and why should i add it to a repository?";
    [RuleGuidance] private const string CheckValidity = "How can i check that a gitignore file is valid?";

    private readonly IGpt3Client _gpt3Client;

    public GitIgnoreRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "gitignore";
    public RuleCategory Category => RuleCategory.Quality;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        var diagnostics = await GetDiagnosis();

        async Task<RuleDiagnostics> GetDiagnosis()
        {
            var e = context.GitTree.SingleFileOrDefault(x => x.PathEquals(".gitignore"));
            if (e is null) return new RuleDiagnostics(Diagnosis.Error, "missing", "");

            var fileContent = await context.GetFile(".gitignore");
            var ignore = new Ignore.Ignore();
            ignore.Add(fileContent.Split("\n"));

            var ignoredFiles = new List<string>();
            context.GitTree.AnalyzeRecursive(
                x => ignore.IsIgnored(x.Item.Path),
                (
                    x,
                    _) => ignoredFiles.Add(x.Item.Path));

            var visualCount = ignoredFiles.Count > 50 ? 50 : ignoredFiles.Count;
            var details = ignoredFiles.Any()
                ? $@"
According to the gitignore rules at the root of this repo,
these files should not exist in the repo. See {Shared.CreateEmbeddedLink("https://github.com/github/gitignore", "Recommended Ignore Files")}
<br/>
Showing first {visualCount} files:
<br/>
{string.Join("<br/>", ignoredFiles.Take(visualCount))}"
                : "";

            return ignoredFiles.Any()
                ? new RuleDiagnostics(Diagnosis.Warning, "found but contains violations", details, e.GetLink(context))
                : new RuleDiagnostics(Diagnosis.Info, "found and without any violations", details, e.GetLink(context));
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(WhereToFind, MultipleFiles, CheckValidity),
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("about git ignore", "https://docs.github.com/en/get-started/getting-started-with-git/ignoring-files")
        });
    }
}