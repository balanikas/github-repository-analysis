using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Quality;

internal class DockerIgnoreRuleApplicator : IRuleApplicator
{
    public string RuleName => "dockerignore";
    public RuleCategory Category => RuleCategory.Quality;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            var file = context.GitTree.FirstFileOrDefaultRecursive(x => x.HasFileName("Dockerfile"));
            var ignore = context.GitTree.FirstFileOrDefaultRecursive(x => x.HasFileName(".dockerignore"));

            if (file is not null && ignore is not null)
                return new(Diagnosis.Info, "found docker file and docker ignore", null, file.GetLink(context));
            if (file is not null && ignore is null)
                return new(Diagnosis.Warning, "found docker file but no docker ignore", null, file.GetLink(context));
            if (file is null && ignore is not null)
                return new(Diagnosis.Warning, "found docker ignore but no docker file");
            return new(Diagnosis.NotApplicable, "not found");
        }

        return Rule.Create(this, diagnostics, new()
        {
            Text = @"
Before the docker CLI sends the context to the docker daemon, it looks for a file named .dockerignore in the root directory 
of the context. If this file exists, the CLI modifies the context to exclude files and directories that match patterns in it. 
This helps to avoid unnecessarily sending large or sensitive files and directories to the daemon and potentially adding them 
to images using ADD or COPY.",
            AboutLink = new("about Dockerfile", "https://docs.docker.com/develop/develop-images/dockerfile_best-practices/")
        });
    }
}