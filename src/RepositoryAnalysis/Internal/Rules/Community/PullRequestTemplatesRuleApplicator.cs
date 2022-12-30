using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class PullRequestTemplatesRuleApplicator : IRuleApplicator
{
    public string RuleName => "pull request templates";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            if (context.Repo.PullRequestTemplates != null && context.Repo.PullRequestTemplates.Any())
            {
                var names = context.Repo.PullRequestTemplates.Select(x => x.Filename);
                var templates = "Templates found: <br/>" + string.Join("<br/>", names);
                return new RuleDiagnostics(Diagnosis.Info,
                    $"found {context.Repo.PullRequestTemplates.Count} pull request templates", templates);
            }

            return new RuleDiagnostics(Diagnosis.Warning, "missing pull request templates");
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            Text = @"
Pull requests let you tell others about changes you've pushed to a branch in a repository on GitHub. 
Once a pull request is opened, you can discuss and review the potential changes with collaborators and add follow-up commits before your changes are merged into the base branch.",
            AboutUrl =
                "https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests",
            AboutHeader = "about pull requests",
            GuidanceUrl =
                "https://docs.github.com/en/communities/using-templates-to-encourage-useful-issues-and-pull-requests/creating-a-pull-request-template-for-your-repository",
            GuidanceHeader = "how to add a pull request template"
        });
    }
}