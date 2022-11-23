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
        var langKey = _context.Repo.PrimaryLanguage?.Name.ToLower();
        if (langKey is null)
        {
            Console.WriteLine("no primary language detected");
            return Array.Empty<Rule>();
        }

        if (!_languageRulesMap.ContainsKey(langKey)) return await Task.FromResult(Array.Empty<Rule>());

        var rules = _languageRulesMap[langKey]();
        return await Task.FromResult(rules);
    }

    private Rule GetSolutionStructureRule()
    {
        var warnings = new List<string>();
        Shared.AnalyzeRecursive(
            _context.RootEntries,
            x => x.HasExtension(".sln"),
            (
                entry,
                entries) =>
            {
                if (entries.Any(x => x.HasExtension(".cs")))
                    warnings.Add($"detected source files next to the solution file {Shared.CreateLink(Shared.GetEntryUrl(_context, entry)!, entry.Path)}");

                if (entries.Any(x => x.HasExtension(".csproj")))
                    warnings.Add($"detected project files next to the solution file {Shared.CreateLink(Shared.GetEntryUrl(_context, entry)!, entry.Path)}");
            });

        var csFile = Shared.GetFirstBlob(_context.RootEntries, x => x.HasExtension(".cs"));
        if (csFile is not null) warnings.Add("found code file in root folder. Consider adding a /src folder to contain code files");

        var csProjFile = Shared.GetFirstBlob(_context.RootEntries, x => x.HasExtension(".csproj"));
        if (csProjFile is not null) warnings.Add("found project file in root folder. Consider adding a /src folder to contain project files");

        if (_context.RootEntries.Count < 5) warnings.Add("Root folder looks incomplete. Essential files can be added.");

        var (diagnosis, note) = GetDiagnosis(warnings);
        return Rule.DotnetSolutionStructure(diagnosis, note, $@"
Found {warnings.Count} issues.
<br/>
{string.Join("<br/>", warnings)}
");

        (Diagnosis, string) GetDiagnosis(
            IReadOnlyList<string> e)
        {
            return e.Any()
                ? (Diagnosis.Warning, $"found {e.Count()} potential solution structure issues")
                : (Diagnosis.Info, "did not find any issues");
        }
    }


    private Rule GetRulesetRule()
    {
        var entries = Shared.GetBlobsRecursive(_context.RootEntries, x => x.HasExtension(".ruleset"));
        var details = $@"
Found these rulesets:
<br/>
{string.Join("<br/>", entries.Select(x => Shared.GetEntryUrl(_context, x)))}
";
        var (diagnosis, note) = GetDiagnosis(entries);
        return Rule.Ruleset(diagnosis, note, details);


        (Diagnosis, string) GetDiagnosis(
            IEnumerable<GitHubGraphQlClient.Entry> e)
        {
            return e.Any()
                ? (Diagnosis.Warning, $"found {e.Count()} .ruleset files")
                : (Diagnosis.Info, "did not find ruleset files");
        }
    }

    private Rule GetDotnetTestsRule()
    {
        var testProjects = Shared.GetBlobsRecursive(_context.RootEntries, IsTestProject).ToArray();
        var testFiles = Shared.GetBlobsRecursive(_context.RootEntries, IsTestFile).ToArray();
        var (diagnosis, note) = GetDiagnosis(testProjects, testFiles);
        var details = $@"
Detected {testProjects.Length} test projects.
Detected {testFiles.Length} test files.
";
        return Rule.DotnetTests(diagnosis, note, details);

        (Diagnosis, string) GetDiagnosis(
            IEnumerable<GitHubGraphQlClient.Entry> projects,
            IEnumerable<GitHubGraphQlClient.Entry> files)
        {
            return !projects.Any() || !files.Any()
                ? (Diagnosis.Warning, "found issues")
                : (Diagnosis.Info, "found");
        }

        bool IsTestProject(
            GitHubGraphQlClient.Entry x)
        {
            return x.PathEndsWith(".csproj") && x.ParentPathEndsWith("test", "tests", "spec", "specs");
        }

        bool IsTestFile(
            GitHubGraphQlClient.Entry x)
        {
            return x.PathEndsWith(".cs") switch
            {
                true when x.PathEndsWith("test.cs", "tests.cs", "spec.cs", "specs.cs") => true,
                _ => false
            };
        }
    }
}