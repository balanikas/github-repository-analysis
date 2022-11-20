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
            GetContributingRule(),
            GetCodeOfConductRule(),
            GetCodeOwnersRule(),
            GetIssuesRule(),
            GetPullRequestsRule(),
            GetDiscussionsRule(),
            GetProjectsRule(),
            GetSupportRule(),
            GetCitationRule()
        };

        return await Task.FromResult(rules);
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
        var entry = Shared.GetBlob(_context.RootEntries,
            x => citationFileNames.Contains(x.Path, StringComparer.OrdinalIgnoreCase));
        var (diagnosis, note) = GetDiagnosis(entry);

        return new Rule
        {
            Name = "Citation file",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Text = @"
You can add a CITATION file to your repository to help users correctly cite your software.",
                AboutUrl =
                    "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-citation-files",
                AboutHeader = "about citation files",
                GuidanceUrl = "https://docs.github",
                GuidanceHeader = "how to add a citation file"
            }
        };

        (Diagnosis, string) GetDiagnosis(
            GitHubApi.Entry? e)
        {
            return e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Warning, "missing citation file");
        }
    }

    private Rule GetSupportRule()
    {
        var entry = Shared.GetBlobRecursive(_context.RootEntries,
            x => x.Path.Equals("support", StringComparison.OrdinalIgnoreCase) ||
                 x.Path.Equals("docs/support", StringComparison.OrdinalIgnoreCase) ||
                 x.Path.Equals(".github/support", StringComparison.OrdinalIgnoreCase));
        var (diagnosis, note) = GetDiagnosis(entry);

        return new Rule
        {
            Name = "Support file",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Text = @"
You can create a SUPPORT file to let people know about ways to get help with your project.
To direct people to specific support resources, you can add a SUPPORT file to your repository's root, docs, or .github folder. 
When someone creates an issue in your repository, they will see a link to your project's SUPPORT file.",
                AboutUrl = "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/adding-support-resources-to-your-project",
                AboutHeader = "about support files",
                GuidanceUrl = "https://docs.github",
                GuidanceHeader = "how to add a support file"
            }
        };

        (Diagnosis, string) GetDiagnosis(
            GitHubApi.Entry? e)
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

        return new Rule
        {
            Name = "Pull requests",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Text = @"
Pull requests let you tell others about changes you've pushed to a branch in a repository on GitHub. 
Once a pull request is opened, you can discuss and review the potential changes with collaborators and add follow-up commits before your changes are merged into the base branch.",
                AboutUrl =
                    "https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests",
                AboutHeader = "about pull requests",
                GuidanceUrl = "https://docs.github",
                GuidanceHeader = "how to add a pull request template"
            }
        };

        (Diagnosis, string) GetDiagnosis()
        {
            return _context.Repo.PullRequestTemplates.Any()
                ? (Diagnosis.Info,
                    $"found {_context.Repo.PullRequests.TotalCount} pull requests and {_context.Repo.PullRequests.TotalCount} pull request templates")
                : (Diagnosis.Warning, "missing pull request templates");
        }
    }

    private Rule GetProjectsRule()
    {
        var (diagnosis, note) = GetDiagnosis();


        return new Rule
        {
            Name = "Projects",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Text = @"
A project is an adaptable spreadsheet that integrates with your issues and pull requests on GitHub to help you plan and track your work effectively. 
You can create and customize multiple views by filtering, sorting, grouping your issues and pull requests, adding custom fields to track metadata specific 
to your team, and visualize work with configurable charts. Rather than enforcing a specific methodology, 
a project provides flexible features you can customize to your team’s needs and processes.",
                AboutUrl = "https://docs.github.com/en/issues/planning-and-tracking-with-projects/learning-about-projects/about-projects",
                AboutHeader = "about projects",
                GuidanceUrl = "https://docs.github",
                GuidanceHeader = "how to work with projects"
            }
        };

        (Diagnosis, string) GetDiagnosis()
        {
            return _context.Repo.HasProjectsEnabled
                ? (Diagnosis.Info, "feature is enabled")
                : (Diagnosis.Info, "feature is disabled");
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

        return new Rule
        {
            Name = "Issues",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Text = @"
Issues let you track your work on GitHub, where development happens.
You may wish to turn issues off for your repository if you do not accept contributions or bug reports.",
                AboutUrl = "https://docs.github.com/en/issues/tracking-your-work-with-issues/about-issues",
                AboutHeader = "about issues",
                GuidanceUrl = "https://docs.github",
                GuidanceHeader = "how to work with issues"
            }
        };

        (Diagnosis, string) GetDiagnosis()
        {
            return _context.Repo.HasIssuesEnabled
                ? _context.Repo.IssueTemplates.Any()
                    ? (Diagnosis.Info, $"found {_context.Repo.Issues.TotalCount} issues and {_context.Repo.IssueTemplates.Count} issue templates")
                    : (Diagnosis.Warning, "issues are enabled but missing issue templates")
                : (Diagnosis.Warning, "feature is disabled");
        }
    }

    private Rule GetDiscussionsRule()
    {
        var (diagnosis, note) = GetDiagnosis();
        return new Rule
        {
            Name = "Discussions",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Text = @"
Use discussions to ask and answer questions, share information, make announcements, and conduct or participate in a conversation about a project on GitHub.
With GitHub Discussions, the community for your project can create and participate in conversations within the project's repository or organization. 
Discussions empower a project's maintainers, contributors, and visitors to gather and accomplish the following goals in a central location, without third-party tools.",
                AboutUrl = "https://docs.github.com/en/discussions/collaborating-with-your-community-using-discussions/about-discussions",
                AboutHeader = "about discussions"
            }
        };

        (Diagnosis, string) GetDiagnosis()
        {
            return _context.Repo.HasDiscussionsEnabled
                ? (Diagnosis.Info, "feature is enabled")
                : (Diagnosis.Warning, "feature is disabled");
        }
    }

    private Rule GetCodeOwnersRule()
    {
        var entry = Shared.GetBlobRecursive(_context.RootEntries, x =>
            Path.GetFileName(x.Path).Equals("codeowners", StringComparison.OrdinalIgnoreCase));

        var (diagnosis, note) = GetDiagnosis(entry);
        return new Rule
        {
            Name = "Code owners",
            Note = note,
            Diagnosis = diagnosis,
            ResourceName = entry?.Path,
            ResourceUrl = Shared.GetEntryUrl(_context, entry),
            Explanation = new Explanation
            {
                Text = @"
You can use a CODEOWNERS file to define individuals or teams that are responsible for code in a repository.",
                AboutUrl =
                    "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-code-owners",
                AboutHeader = "about code owners"
            }
        };

        (Diagnosis, string) GetDiagnosis(
            GitHubApi.Entry? e)
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
        return new Rule
        {
            Name = "Code of conduct",
            Note = note,
            Diagnosis = diagnosis,
            ResourceName = entry?.Name,
            ResourceUrl = entry?.Url,
            Explanation = new Explanation
            {
                Text = @"
Adopt a code of conduct to define community standards, signal a welcoming and inclusive project, and outline procedures for handling abuse.
A code of conduct defines standards for how to engage in a community. It signals an inclusive environment that respects all contributions. 
It also outlines procedures for addressing problems between members of your project's community. ",
                AboutUrl = "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/adding-a-code-of-conduct-to-your-project",
                AboutHeader = "about code of conduct"
            }
        };

        (Diagnosis, string) GetDiagnosis(
            GitHubApi.CodeOfConduct? e)
        {
            return e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Warning, "missing");
        }
    }

    private Rule GetContributingRule()
    {
        var entry = Shared.GetBlob(_context.RootEntries,
            x => x.Path.Equals("contributing", StringComparison.OrdinalIgnoreCase) ||
                 x.Path.Equals("docs/contributing", StringComparison.OrdinalIgnoreCase) ||
                 x.Path.Equals(".github/contributing", StringComparison.OrdinalIgnoreCase));

        var (diagnosis, note) = GetDiagnosis(entry);
        return new Rule
        {
            Name = "Contributing",
            Note = note,
            Diagnosis = diagnosis,
            ResourceName = entry?.Path,
            ResourceUrl = Shared.GetEntryUrl(_context, entry),
            Explanation = new Explanation
            {
                Text = @"
To help your project contributors do good work, you can add a file with contribution guidelines to your project repository's root, docs, or .github folder. 
When someone opens a pull request or creates an issue, they will see a link to that file. The link to the contributing guidelines also appears on your repository's contribute page.",
                AboutUrl =
                    "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/setting-guidelines-for-repository-contributors",
                AboutHeader = "about contributing guidelines"
            }
        };

        (Diagnosis, string) GetDiagnosis(
            GitHubApi.Entry? e)
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