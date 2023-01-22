using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Documentation;

internal class ReadmeRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string HowToWrite = "Write a short example of a well designed readme file";
    [RuleGuidance] private const string MultipleFiles = "How many readme files can a repository have and why should i have more than one?";

    [RuleGuidance(200, Tone.Motivational, Complexity.Simple)]
    private const string WhatIs = "What is a github readme file and why is it important?";

    private readonly IGpt3Client _gpt3Client;

    public ReadmeRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "readme";
    public RuleCategory Category => RuleCategory.Documentation;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        bool IsReadme(GitTree.Node x) =>
            x.HasFileName("readme", "readme.md", "readme.txt", "readme.rst");

        var rootFile = context.GitTree.FirstFileOrDefault(IsReadme);
        var nonRootFiles = context.GitTree.FilesRecursive(IsReadme);

        var details =
            nonRootFiles.Count == 1
                ? ""
                : nonRootFiles.Count > 100
                    ? $"First 100 readme files <br/> {nonRootFiles.GetEmbeddedLinksAsString(context)}"
                    : $"All readme files <br/> {nonRootFiles.GetEmbeddedLinksAsString(context)}";

        var diagnostics = rootFile is not null
            ? rootFile.Item.Size switch
            {
                < 500 => new RuleDiagnostics(Diagnosis.Warning, "root readme is too short (less than 500 chars long)", details, rootFile.GetLink(context)),
                _ => new RuleDiagnostics(Diagnosis.Info, "found root readme root", details, rootFile.GetLink(context))
            }
            : new RuleDiagnostics(Diagnosis.Error, "missing root readme", details);

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(MultipleFiles, HowToWrite),
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("about readmes",
                "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes"),
            GuidanceLink = diagnostics.Diagnosis == Diagnosis.Error ? context.GetCommunityLink() : null
        });
    }
}