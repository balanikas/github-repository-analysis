using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Quality;

public class DockerIgnoreRuleApplicator : IRuleApplicator
{
    public string RuleName => "dockerignore";
    public RuleCategory Category => RuleCategory.Quality;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var dockerFile = context.GitTree.FirstFileOrDefault(x => x.PathEquals("Dockerfile"));
        var dockerIgnore = context.GitTree.FirstFileOrDefault(x => x.PathEquals(".dockerignore"));

        var (diagnosis, note) = GetDiagnosis(dockerFile, dockerIgnore);
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
Before the docker CLI sends the context to the docker daemon, it looks for a file named .dockerignore in the root directory 
of the context. If this file exists, the CLI modifies the context to exclude files and directories that match patterns in it. 
This helps to avoid unnecessarily sending large or sensitive files and directories to the daemon and potentially adding them 
to images using ADD or COPY.",
                    AboutUrl = "https://docs.docker.com/develop/develop-images/dockerfile_best-practices/",
                    AboutHeader = "about Dockerfile"
                }
            } with
            {
                ResourceName = dockerFile?.Item.Path, ResourceUrl = dockerFile.GetUrl(context)
            };

        (Diagnosis, string) GetDiagnosis(
            GitTree.Node? file,
            GitTree.Node? ignore) =>
            file is not null && ignore is not null
                ? (Diagnosis.Info, "found docker file and docker ignore")
                : file is not null && ignore is null
                    ? (Diagnosis.Warning, "found docker file but no docker ignore")
                    : (Diagnosis.Info, "not found");
    }
}