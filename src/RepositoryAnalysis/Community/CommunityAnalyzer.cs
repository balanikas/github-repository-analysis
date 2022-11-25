using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Community;

public class CommunityAnalyzer
{
    private readonly AnalysisContext _context;

    public CommunityAnalyzer(
        AnalysisContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Rule>> Analyze()
    {
        var rules = new List<Rule>
        {
            GetLicenseRule(),
            GetContributingRule(),
            GetCodeOfConductRule(),
            GetCodeOwnersRule(),
            GetIssuesRule(),
            GetPullRequestsRule(),
            GetDiscussionsRule(),
            GetSupportRule(),
            GetCitationRule()
        };

        return await Task.FromResult(rules);
    }

    private Rule GetLicenseRule()
    {
        var license = _context.Repo.LicenseInfo;
        var (diagnosis, note) = GetDiagnosis(license);

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.LicenseInfo? e)
        {
            return e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Error, "missing");
        }

        return Rule.License(diagnosis, note) with { ResourceName = license?.Name, ResourceUrl = license?.Url };
    }
    
    private Rule GetCitationRule()
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
        var entry = Shared.GetFirstBlob(_context.RootEntries,
            x => citationFileNames.Contains(x.Path, StringComparer.OrdinalIgnoreCase));
        var (diagnosis, note) = GetDiagnosis(entry);

        return Rule.CitationFile(diagnosis, note);

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.Entry? e)
        {
            return e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Warning, "missing citation file");
        }
    }

    private Rule GetSupportRule()
    {
        var entry = Shared.GetBlobRecursive(_context.RootEntries,
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

    private Rule GetPullRequestsRule()
    {
        var (diagnosis, note) = GetDiagnosis();
        return Rule.PullRequests(diagnosis, note);

        (Diagnosis, string) GetDiagnosis()
        {
            return _context.Repo.PullRequestTemplates.Any()
                ? (Diagnosis.Info,
                    $"found {_context.Repo.PullRequests.TotalCount} pull requests and {_context.Repo.PullRequestTemplates.Count} pull request templates")
                : (Diagnosis.Warning, "missing pull request templates");
        }
    }


    private Rule GetIssuesRule()
    {
        var (diagnosis, note) = GetDiagnosis();

        var templates = "";
        if (_context.Repo.IssueTemplates.Any())
        {
            var links = _context.Repo.IssueTemplates.Select(x => Shared.CreateIssueTemplateLink(_context, x.Filename, x.Name));
            templates = "Templates found: <br/>" + string.Join("<br/>", links);
        }

        return Rule.Issues(diagnosis, note, templates);

        (Diagnosis, string) GetDiagnosis()
        {
            return _context.Repo.HasIssuesEnabled
                ? _context.Repo.IssueTemplates.Any()
                    ? (Diagnosis.Info, $"found {_context.Repo.Issues.TotalCount} issues and {_context.Repo.IssueTemplates.Count} issue templates")
                    : (Diagnosis.Warning, "issues are enabled but missing issue templates")
                : (Diagnosis.Info, "feature is disabled");
        }
    }

    private Rule GetDiscussionsRule()
    {
        var (diagnosis, note) = GetDiagnosis();
        return Rule.Discussions(diagnosis, note);

        (Diagnosis, string) GetDiagnosis()
        {
            return _context.Repo.HasDiscussionsEnabled
                ? (Diagnosis.Info, "feature is enabled")
                : (Diagnosis.Info, "feature is disabled");
        }
    }

    private Rule GetCodeOwnersRule()
    {
        var entry = Shared.GetBlobRecursive(_context.RootEntries, x =>
            Path.GetFileName(x.Path).Equals("codeowners", StringComparison.OrdinalIgnoreCase));

        var (diagnosis, note) = GetDiagnosis(entry);
        return Rule.CodeOwners(diagnosis, note) with { ResourceName = entry?.Path, ResourceUrl = Shared.GetEntryUrl(_context, entry) };

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.Entry? e)
        {
            return e is not null
                ? _context.Repo.Codeowners.Errors.Any()
                    ? (Diagnosis.Warning, $"{_context.Repo.Codeowners.Errors.Count} errors in {e.Path}")
                    : (Diagnosis.Info, "")
                : (Diagnosis.Warning, "missing code owners file");
        }
    }

    private Rule GetCodeOfConductRule()
    {
        var entry = _context.Repo.CodeOfConduct;

        var (diagnosis, note) = GetDiagnosis(entry);
        return Rule.CodeOfConduct(diagnosis, note) with { ResourceName = entry?.Name, ResourceUrl = entry?.Url };

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.CodeOfConduct? e)
        {
            return e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Warning, "missing");
        }
    }

    private Rule GetContributingRule()
    {
        var entry = Shared.GetSingleBlob(_context.RootEntries,
            x => x.PathEquals(
                "contributing.md",
                "docs/contributing.md",
                ".github/contributing.md",
                "contributing.rst",
                "docs/contributing.rst",
                ".github/contributing.rst"
            ));

        var (diagnosis, note) = GetDiagnosis(entry);
        return Rule.Contributing(diagnosis, note) with { ResourceName = entry?.Path, ResourceUrl = Shared.GetEntryUrl(_context, entry) };

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