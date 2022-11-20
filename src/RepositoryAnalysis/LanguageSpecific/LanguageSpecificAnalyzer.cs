using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.LanguageSpecific;

public class LanguageSpecificAnalyzer
{
    private readonly AnalysisContext _context;

    private readonly IDictionary<string, Func<IReadOnlyList<Rule>>> _languageRulesMap;

    public LanguageSpecificAnalyzer(
        AnalysisContext context)
    {
        _context = context;
        _languageRulesMap = new Dictionary<string, Func<IReadOnlyList<Rule>>>
        {
            {
                "c#", () => new List<Rule>
                {
                    GetRulesetRule(),
                    GetDotnetTestsRule(),
                    GetSolutionStructureRule()
                }
            }
        };
    }


    public async Task<IReadOnlyList<Rule>> Analyze()
    {
        var langKey = _context.Repo.PrimaryLanguage.Name.ToLower();
        if (!_languageRulesMap.ContainsKey(langKey)) return await Task.FromResult(Array.Empty<Rule>());

        var rules = _languageRulesMap[langKey]();
        return await Task.FromResult(rules);
    }

    private Rule GetSolutionStructureRule()
    {
        var warnings = new List<string>();
        Shared.AnalyzeRecursive(
            _context.RootEntries,
            x => Path.GetExtension(x.Path).Equals(".sln", StringComparison.OrdinalIgnoreCase),
            (
                entry,
                entries) =>
            {
                if (entries.Any(x => Path.GetExtension(x.Path).Equals(".cs", StringComparison.OrdinalIgnoreCase)))
                    warnings.Add($"detected source files next to the solution file {Shared.CreateLink(Shared.GetEntryUrl(_context, entry)!, entry.Path)}");

                if (entries.Any(x => Path.GetExtension(x.Path).Equals(".csproj", StringComparison.OrdinalIgnoreCase)))
                    warnings.Add($"detected project files next to the solution file {Shared.CreateLink(Shared.GetEntryUrl(_context, entry)!, entry.Path)}");
            });

        var csFile = Shared.GetBlob(_context.RootEntries,
            x => Path.GetExtension(x.Path).Equals(".cs", StringComparison.OrdinalIgnoreCase));
        if (csFile is not null) warnings.Add("found code file in root folder. Consider adding a /src folder to contain code files");

        var csProjFile = Shared.GetBlob(_context.RootEntries,
            x => Path.GetExtension(x.Path).Equals(".csproj", StringComparison.OrdinalIgnoreCase));
        if (csProjFile is not null) warnings.Add("found project file in root folder. Consider adding a /src folder to contain project files");

        if (_context.RootEntries.Count < 5) warnings.Add("Root folder looks incomplete. Essential files can be added.");

        var (diagnosis, note) = GetDiagnosis(warnings);
        return new Rule
        {
            Name = "Dotnet solution structure",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Text = $@"
It is good practice to follow standard solution structure conventions. Found {warnings.Count} issues."
            }
        };

        (Diagnosis, string) GetDiagnosis(
            List<string> e)
        {
            return e.Any()
                ? (Diagnosis.Warning, $"found {e.Count()} potential solution structure issues")
                : (Diagnosis.Info, "did not any issues");
        }
    }


    private Rule GetRulesetRule()
    {
        var entries = Shared.GetBlobsRecursive(_context.RootEntries, x => Path.GetExtension(x.Path).Equals(".ruleset", StringComparison.OrdinalIgnoreCase));
        var (diagnosis, note) = GetDiagnosis(entries);
        return new Rule
        {
            Name = "Ruleset",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Text = @"
Ruleset configuration files for static code analysis are being deprecated in favor of more modern tools, 
like EditorConfig and dotnet analyzers.",
                AboutUrl = "https://learn.microsoft.com/en-us/visualstudio/code-quality/analyzers-faq?view=vs-2022",
                AboutHeader = "about dotnet analyzers",
                GuidanceUrl = "https://docs.github",
                GuidanceHeader = "how to work with analyzers"
            }
        };

        (Diagnosis, string) GetDiagnosis(
            IEnumerable<GitHubApi.Entry> e)
        {
            return e.Any()
                ? (Diagnosis.Error, $"found {e.Count()} .ruleset files")
                : (Diagnosis.Info, "did not find ruleset files");
        }
    }

    private Rule GetDotnetTestsRule()
    {
        var testProjects = Shared.GetBlobsRecursive(_context.RootEntries, IsTestProject).ToArray();
        var testFiles = Shared.GetBlobsRecursive(_context.RootEntries, IsTestFile).ToArray();
        var (diagnosis, note) = GetDiagnosis(testProjects, testFiles);

        return new Rule
        {
            Name = "Tests",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Text = $@"
Tests increase the quality of software. 
<br/>
Detected {testProjects.Length} test projects.
Detected {testFiles.Length} test files.",
                AboutUrl = "https://learn.microsoft.com/en-us/dotnet/core/testing/",
                AboutHeader = "testing in dotnet"
            }
        };

        (Diagnosis, string) GetDiagnosis(
            IEnumerable<GitHubApi.Entry> projects,
            IEnumerable<GitHubApi.Entry> files)
        {
            return !projects.Any() || !files.Any()
                ? (Diagnosis.Warning, "found issues")
                : (Diagnosis.Info, "");
        }

        bool IsTestProject(
            GitHubApi.Entry x)
        {
            var isProjectFile = x.Path.EndsWith(".csproj");
            var parentName = Path.GetDirectoryName(x.Path);
            if (isProjectFile && parentName is not null)
                if (parentName.EndsWith("test", StringComparison.OrdinalIgnoreCase) || parentName.EndsWith("tests", StringComparison.OrdinalIgnoreCase) ||
                    parentName.EndsWith("spec", StringComparison.OrdinalIgnoreCase) || parentName.EndsWith("specs", StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        bool IsTestFile(
            GitHubApi.Entry x)
        {
            var isCodeFile = x.Path.EndsWith(".cs");

            if (isCodeFile)
                if (x.Path.EndsWith("test.cs", StringComparison.OrdinalIgnoreCase) || x.Path.EndsWith("tests.cs", StringComparison.OrdinalIgnoreCase) ||
                    x.Path.EndsWith("spec.cs", StringComparison.OrdinalIgnoreCase) || x.Path.EndsWith("specs.cs", StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }
    }
}