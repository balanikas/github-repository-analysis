using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

public class PullRequestsRuleApplicator : IRuleApplicator
{
    public string RuleName => "pull requests";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var (diagnosis, note) = GetDiagnosis();
        var templates = "";
        if (context.Repo.PullRequestTemplates.Any())
        {
            var names = context.Repo.PullRequestTemplates.Select(x => x.Filename);
            templates = "Templates found: <br/>" + string.Join("<br/>", names);
        }

        return new Rule
        {
            Name = RuleName,
            Category = Category,
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = templates,
                Text = @"
Pull requests let you tell others about changes you've pushed to a branch in a repository on GitHub. 
Once a pull request is opened, you can discuss and review the potential changes with collaborators and add follow-up commits before your changes are merged into the base branch.",
                AboutUrl =
                    "https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests",
                AboutHeader = "about pull requests",
                GuidanceUrl = "https://docs.github.com/en/communities/using-templates-to-encourage-useful-issues-and-pull-requests/creating-a-pull-request-template-for-your-repository",
                GuidanceHeader = "how to add a pull request template"
            }
        };

        (Diagnosis, string) GetDiagnosis() =>
            context.Repo.PullRequestTemplates.Any()
                ? (Diagnosis.Info,
                    $"found {context.Repo.PullRequests.TotalCount} pull requests and {context.Repo.PullRequestTemplates.Count} pull request templates")
                : (Diagnosis.Warning, "missing pull request templates");
    }
}