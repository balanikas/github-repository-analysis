using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Documentation;

public class DocumentationAnalyzer
{
    private readonly AnalysisContext _context;

    public DocumentationAnalyzer(
        AnalysisContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Rule>> Analyze()
    {
        var rules = new List<Rule>
        {
            GetReadmeRule(),
            GetDescriptionRule(),
            GetHomePageUrlRule(),
            GetChangeLogRule(),
            GetTopicsRule()
        };
        return await Task.FromResult(rules);
    }

    private Rule GetTopicsRule()
    {
        var (diagnosis, note) = GetDiagnosis();
        return new Rule
        {
            Name = "Topics",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Text = @"
To help other people find and contribute to your project, you can add topics to your repository related to your project's intended purpose, subject area, affinity groups, or other important qualities.",
                AboutUrl =
                    "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/classifying-your-repository-with-topics",
                AboutHeader = "about topics",
                GuidanceUrl = "https://docs.github",
                GuidanceHeader = "how to work with topics"
            }
        };

        (Diagnosis, string) GetDiagnosis()
        {
            return _context.Repo.RepositoryTopics.TotalCount > 0
                ? (Diagnosis.Info, $"found {_context.Repo.RepositoryTopics.TotalCount} topics")
                : (Diagnosis.Warning, "no topics found");
        }
    }

    private Rule GetChangeLogRule()
    {
        var entry = Shared.GetBlob(_context.RootEntries, x => x.Path.Contains("changelog", StringComparison.OrdinalIgnoreCase));
        var (diagnosis, note) = GetDiagnosis(entry);
        return new Rule
        {
            Name = "Change log",
            Note = note,
            Diagnosis = diagnosis,
            ResourceName = entry?.Path,
            ResourceUrl = Shared.GetEntryUrl(_context, entry),
            Explanation = new Explanation
            {
                Text = @"
A changelog is a kind of summary of all your changes. 
It should be easy to understand both by the users using your project and the developers working on it."
            }
        };

        (Diagnosis, string) GetDiagnosis(
            GitHubApi.Entry? e)
        {
            return e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Warning, "missing");
        }
    }

    private Rule GetHomePageUrlRule()
    {
        var (diagnosis, note) = GetDiagnosis();
        //todo: check head request if url exists
        return new Rule
        {
            Name = "Homepage",
            Note = note,
            Diagnosis = diagnosis,
            ResourceName = _context.Repo.HomepageUrl,
            ResourceUrl = _context.Repo.HomepageUrl,
            Explanation = new Explanation
            {
                Text = @"
A repository homepage url helps users to get more information about the project."
            }
        };

        (Diagnosis, string) GetDiagnosis()
        {
            return !string.IsNullOrEmpty(_context.Repo.HomepageUrl)
                ? (Diagnosis.Info, "found homepage")
                : (Diagnosis.Warning, "missing");
        }
    }

    private Rule GetDescriptionRule()
    {
        var (diagnosis, note) = GetDiagnosis();

        return new Rule
        {
            Name = "Description/About",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Text = @"
A repository description helps users to understand what the repository is about.",
                AboutUrl = "https://docs.github.com/en/get-started/quickstart/create-a-repo",
                AboutHeader = "about topics",
                GuidanceUrl = "https://docs.github.com/en/get-started/quickstart/create-a-repo",
                GuidanceHeader = "this guide on how to create a repository"
            }
        };

        (Diagnosis, string) GetDiagnosis()
        {
            return _context.Repo.Description is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Warning, "missing");
        }
    }

    private Rule GetReadmeRule()
    {
        //todo: find multiple? no, because readme should exist at least in root even if there are more readmes
        var entry = Shared.GetBlob(_context.RootEntries, x => x.Path.Contains("readme", StringComparison.OrdinalIgnoreCase));
        var (diagnosis, note) = GetDiagnosis(entry);

        return new Rule
        {
            Name = "Readme",
            Note = note,
            Diagnosis = diagnosis,
            ResourceName = entry?.Path,
            ResourceUrl = Shared.GetEntryUrl(_context, entry),
            Explanation = new Explanation
            {
                Text = @"
A repository should contain a readme file, to tell other people why your project is useful, what they can do with your project, and how they can use it.",
                AboutUrl = "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes",
                AboutHeader = "about readmes",
                GuidanceUrl = "https://docs.github",
                GuidanceHeader = "this guide on how to add a readme"
            }
        };

        (Diagnosis, string) GetDiagnosis(
            GitHubApi.Entry? e)
        {
            return e is not null
                ? e.Size switch
                {
                    < 200 => (Diagnosis.Warning, "file content is too short"),
                    _ => (Diagnosis.Info, "found")
                }
                : (Diagnosis.Error, "missing");
        }
    }
}