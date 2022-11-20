using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Quality;

public class QualityAnalyzer
{
    private readonly AnalysisContext _context;

    public QualityAnalyzer(
        AnalysisContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Rule>> Analyze()
    {
        var rules = new List<Rule>
        {
            GetLicenseRule(),
            GetGitIgnoreRule(),
            GetEditorConfigRule(),
            GetLargeFilesRule()
        };

        return await Task.FromResult(rules);
    }


    private Rule GetLicenseRule()
    {
        var license = _context.Repo.LicenseInfo;
        var (diagnosis, note) = GetDiagnosis(license);

        (Diagnosis, string) GetDiagnosis(
            GitHubApi.LicenseInfo? e)
        {
            return e is not null
                ? (Diagnosis.Info, "")
                : (Diagnosis.Error, "missing");
        }

        return new Rule
        {
            Name = "License",
            Note = note,
            Diagnosis = diagnosis,
            ResourceName = license?.Name,
            ResourceUrl = license?.Url,
            Explanation = new Explanation
            {
                Text = @"
Public repositories on GitHub are often used to share open source software. 
For your repository to truly be open source, you'll need to license it so that others are free to use, change, and distribute the software.",
                AboutUrl =
                    "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository",
                AboutHeader = "about open source licensing",
                GuidanceUrl =
                    "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository",
                GuidanceHeader = "how to add a license"
            }
        };
    }

    private Rule GetGitIgnoreRule()
    {
        var entry = Shared.GetBlob(_context.RootEntries, x => x.Path.Equals(".gitIgnore", StringComparison.OrdinalIgnoreCase));
        var (diagnosis, note) = GetDiagnosis(entry);
        return new Rule
        {
            Name = "Git ignore",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Text = @"
You can create a .gitignore file in your repository's root directory to tell Git which files and directories to ignore when you make a commit. 
To share the ignore rules with other users who clone the repository, commit the .gitignore file in to your repository.
Based on the repository language, consider getting one from [link to the appropriate gitignore file]",
                AboutUrl = "https://docs.github.com/en/get-started/getting-started-with-git/ignoring-files",
                AboutHeader = "about git ignore"
            },
            ResourceName = entry?.Path,
            ResourceUrl = Shared.GetEntryUrl(_context, entry)
        };

        (Diagnosis, string) GetDiagnosis(
            GitHubApi.Entry? e)
        {
            return e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Error, "missing"); //todo: advice on which gitignore file to choose based on lang.
        }
    }

    private Rule GetLargeFilesRule()
    {
        var entries = Shared.GetBlobsRecursive(_context.RootEntries, x => x.Size > 10_000_000);
        var (diagnosis, note) = GetDiagnosis(entries);
        return new Rule
        {
            Name = "Large files",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Text = $@"
Large files contained in a repository might be a sign of unoptimized repository. 
<br/>
Some examples:
<br/>
{string.Join("<br/>", entries.Take(3).Select(x => new { x.Path, x.Size }))}",
                AboutUrl = "https://docs.github.com/en/repositories/working-with-files/managing-large-files/about-large-files-on-github",
                AboutHeader = "about large files"
            }
        };

        (Diagnosis, string) GetDiagnosis(
            IEnumerable<GitHubApi.Entry> e)
        {
            return e.Any()
                ? (Diagnosis.Warning, $"found {e.Count()} big files")
                : (Diagnosis.Info, "did not find any big files");
        }
    }


    private Rule GetEditorConfigRule()
    {
        var entry = Shared.GetBlob(_context.RootEntries, x => x.Path.Equals(".editorconfig", StringComparison.OrdinalIgnoreCase));
        var (diagnosis, note) = GetDiagnosis(entry);
        return new Rule
        {
            Name = "Editorconfig",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Text = @"
EditorConfig helps maintain consistent coding styles for multiple developers working on the same project across various editors and IDEs. 
The EditorConfig project consists of a file format for defining coding styles and a collection of text editor plugins that enable editors to read the file format and adhere to defined styles. 
EditorConfig files are easily readable and they work nicely with version control systems.",
                AboutUrl = "https://editorconfig.org/",
                AboutHeader = "about editor config"
            },
            ResourceName = entry?.Path,
            ResourceUrl = Shared.GetEntryUrl(_context, entry)
        };

        (Diagnosis, string) GetDiagnosis(
            GitHubApi.Entry? e)
        {
            return e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Error, "missing");
        }
    }
}