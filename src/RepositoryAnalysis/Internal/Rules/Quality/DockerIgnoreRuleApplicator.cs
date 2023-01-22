using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Quality;

internal class DockerIgnoreRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string WhereToFind = "Where can i find a good dockerignore file for my repository?";
    [RuleGuidance] private const string MultipleFiles = "When should i have multiple dockerignore files in my repository?";

    [RuleGuidance(200, Tone.Motivational, Complexity.Simple)]
    private const string WhatIs = "What is dockerignore and why should i add it to a repository?";

    [RuleGuidance] private const string WhereToPlace = "Where should a dockerignore file be placed?";
    [RuleGuidance] private const string CheckValidity = "How can i check that a dockerignore file is valid?";

    private readonly IGpt3Client _gpt3Client;

    public DockerIgnoreRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "dockerignore";
    public RuleCategory Category => RuleCategory.Quality;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            var file = context.GitTree.FirstFileOrDefaultRecursive(x => x.HasFileName("Dockerfile"));
            var ignore = context.GitTree.FirstFileOrDefaultRecursive(x => x.HasFileName(".dockerignore"));

            if (file is not null && ignore is not null)
                return new RuleDiagnostics(Diagnosis.Info, "found docker file and docker ignore", null, file.GetLink(context));
            if (file is not null && ignore is null)
                return new RuleDiagnostics(Diagnosis.Warning, "found docker file but no docker ignore", null, file.GetLink(context));
            if (file is null && ignore is not null)
                return new RuleDiagnostics(Diagnosis.Warning, "found docker ignore but no docker file");
            return new RuleDiagnostics(Diagnosis.NotApplicable, "not found");
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(WhereToFind, WhereToPlace, MultipleFiles, CheckValidity),
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("about Dockerfile", "https://docs.docker.com/develop/develop-images/dockerfile_best-practices/#exclude-with-dockerignore")
        });
    }
}