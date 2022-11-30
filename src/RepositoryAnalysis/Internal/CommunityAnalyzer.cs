using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

public class CommunityAnalyzer : IAnalyzer
{
    public async Task<IReadOnlyList<Rule>> Analyze(
        AnalysisContext context)
    {
        var rules = new List<Rule>
        {
            GetLicenseRule(context),
            GetContributingRule(context),
            GetCodeOfConductRule(context),
            GetCodeOwnersRule(context),
            GetIssuesRule(context),
            GetPullRequestsRule(context),
            GetDiscussionsRule(context),
            GetSupportRule(context),
            GetCitationRule(context)
        };

        return await Task.FromResult(rules);
    }

    private Rule GetLicenseRule(
        AnalysisContext context)
    {
        var license = context.Repo.LicenseInfo;
        var (diagnosis, note) = GetDiagnosis(license);

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.LicenseInfo? e) =>
            e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Error, "missing");

        return Rule.License(diagnosis, note) with { ResourceName = license?.Name, ResourceUrl = license?.Url };
    }

    private Rule GetCitationRule(
        AnalysisContext context)
    {
        var citationFileNames = new[]
        {
            "CITATION",
            "CITATIONS",
            "CITATION.bib",
            "CITATIONS.bib",
            "CITATION.md",
            "CITATIONS.md",
            "CITATION.cff"
        };
        var entry = Shared.GetFirstBlob(context.RootEntries,
            x => citationFileNames.Contains(x.Path, StringComparer.OrdinalIgnoreCase));
        var (diagnosis, note) = GetDiagnosis(entry);

        return Rule.CitationFile(diagnosis, note);

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.Entry? e) =>
            e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Warning, "missing citation file");
    }

    private Rule GetSupportRule(
        AnalysisContext context)
    {
        var entry = Shared.GetSingleBlobRecursive(context.RootEntries,
            x => x.PathEquals("support") ||
                 x.PathEquals("docs/support") ||
                 x.PathEquals(".github/support"));
        var (diagnosis, note) = GetDiagnosis(entry);

        return Rule.SupportFile(diagnosis, note);

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.Entry? e)
        {
            return e is not null
                ? e.Size switch
                {
                    < 100 => (Diagnosis.Warning, "content is too short"),
                    _ => (Diagnosis.Info, "found")
                }
                : (Diagnosis.Warning, "missing support file");
        }
    }

    private Rule GetPullRequestsRule(
        AnalysisContext context)
    {
        var (diagnosis, note) = GetDiagnosis();
        return Rule.PullRequests(diagnosis, note);

        (Diagnosis, string) GetDiagnosis() =>
            context.Repo.PullRequestTemplates.Any()
                ? (Diagnosis.Info,
                    $"found {context.Repo.PullRequests.TotalCount} pull requests and {context.Repo.PullRequestTemplates.Count} pull request templates")
                : (Diagnosis.Warning, "missing pull request templates");
    }


    private Rule GetIssuesRule(
        AnalysisContext context)
    {
        var (diagnosis, note) = GetDiagnosis();

        var templates = "";
        if (context.Repo.IssueTemplates.Any())
        {
            var links = context.Repo.IssueTemplates.Select(x => Shared.CreateIssueTemplateLink(context, x.Filename, x.Name));
            templates = "Templates found: <br/>" + string.Join("<br/>", links);
        }

        return Rule.Issues(diagnosis, note, templates);

        (Diagnosis, string) GetDiagnosis() =>
            context.Repo.HasIssuesEnabled
                ? context.Repo.IssueTemplates.Any()
                    ? (Diagnosis.Info, $"found {context.Repo.Issues.TotalCount} issues and {context.Repo.IssueTemplates.Count} issue templates")
                    : (Diagnosis.Warning, "issues are enabled but missing issue templates")
                : (Diagnosis.Info, "feature is disabled");
    }

    private Rule GetDiscussionsRule(
        AnalysisContext context)
    {
        var (diagnosis, note) = GetDiagnosis();
        return Rule.Discussions(diagnosis, note);

        (Diagnosis, string) GetDiagnosis() =>
            context.Repo.HasDiscussionsEnabled
                ? (Diagnosis.Info, "feature is enabled")
                : (Diagnosis.Info, "feature is disabled");
    }

    private Rule GetCodeOwnersRule(
        AnalysisContext context)
    {
        var entry = Shared.GetSingleBlobRecursive(context.RootEntries, x =>
            Path.GetFileName(x.Path).Equals("codeowners", StringComparison.OrdinalIgnoreCase));

        var (diagnosis, note) = GetDiagnosis(entry);
        return Rule.CodeOwners(diagnosis, note) with { ResourceName = entry?.Path, ResourceUrl = Shared.GetEntryUrl(context, entry) };

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.Entry? e) =>
            e is not null
                ? context.Repo.Codeowners.Errors.Any()
                    ? (Diagnosis.Warning, $"{context.Repo.Codeowners.Errors.Count} errors in {e.Path}")
                    : (Diagnosis.Info, "")
                : (Diagnosis.Warning, "missing code owners file");
    }

    private Rule GetCodeOfConductRule(
        AnalysisContext context)
    {
        var entry = context.Repo.CodeOfConduct;

        var (diagnosis, note) = GetDiagnosis(entry);
        return Rule.CodeOfConduct(diagnosis, note) with { ResourceName = entry?.Name, ResourceUrl = entry?.Url };

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.CodeOfConduct? e) =>
            e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Warning, "missing");
    }

    private Rule GetContributingRule(
        AnalysisContext context)
    {
        var entry = Shared.GetSingleBlob(context.RootEntries,
            x => x.PathEquals(
                "contributing.md",
                "docs/contributing.md",
                ".github/contributing.md",
                "contributing.rst",
                "docs/contributing.rst",
                ".github/contributing.rst"
            ));

        var (diagnosis, note) = GetDiagnosis(entry);
        return Rule.Contributing(diagnosis, note) with { ResourceName = entry?.Path, ResourceUrl = Shared.GetEntryUrl(context, entry) };

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.Entry? e)
        {
            return e is not null
                ? e.Size switch
                {
                    < 100 => (Diagnosis.Warning, "content is too short"),
                    _ => (Diagnosis.Info, "found")
                }
                : (Diagnosis.Warning, "missing contributing file");
        }
    }
}