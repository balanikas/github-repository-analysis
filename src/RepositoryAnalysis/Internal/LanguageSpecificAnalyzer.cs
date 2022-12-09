using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

public class LanguageSpecificAnalyzer : IAnalyzer
{
    private readonly ILogger<LanguageSpecificAnalyzer> _logger;
    private readonly IDictionary<string, Func<AnalysisContext, IReadOnlyList<Rule>>> _languageRulesMap;

    public LanguageSpecificAnalyzer(ILogger<LanguageSpecificAnalyzer> logger)
    {
        _logger = logger;
        _languageRulesMap = new Dictionary<string, Func<AnalysisContext, IReadOnlyList<Rule>>>
        {
            {
                "c#", context => new List<Rule>
                {
                    _logger.LogPerf(() => GetRulesetRule(context)),
                    _logger.LogPerf(() => GetDotnetTestsRule(context)),
                    _logger.LogPerf(() => GetSolutionStructureRule(context)),
                }
            }
        };
    }


    public async Task<IReadOnlyList<Rule>> Analyze(
        AnalysisContext context)
    {
        var langKey = context.Repo.PrimaryLanguage?.Name.ToLower();
        if (langKey is null)
        {
            Console.WriteLine("no primary language detected");
            return Array.Empty<Rule>();
        }

        if (!_languageRulesMap.ContainsKey(langKey)) return await Task.FromResult(Array.Empty<Rule>());

        var rules = _languageRulesMap[langKey](context);
        return await Task.FromResult(rules);
    }

    private Rule GetSolutionStructureRule(
        AnalysisContext context)
    {
        var warnings = new List<string>();
        context.GitTree.AnalyzeRecursive(
            node => node.HasExtension(".sln"),
            (
                node,
                nodes) =>
            {
                if (nodes.Any(x => x.HasExtension(".cs")))
                    warnings.Add($"detected source files next to the solution file {Shared.CreateLink(node.GetUrl(context)!, node.Item.Path)}");

                if (nodes.Any(x => x.HasExtension(".csproj")))
                    warnings.Add($"detected project files next to the solution file {Shared.CreateLink(node.GetUrl(context)!, node.Item.Path)}");
            });

        var csFile = context.GitTree.FirstFileOrDefault(x => x.HasExtension(".cs"));
        if (csFile is not null) warnings.Add("found code file in root folder. Consider adding a /src folder to contain code files");

        var csProjFile = context.GitTree.FirstFileOrDefault(x => x.HasExtension(".csproj"));
        if (csProjFile is not null) warnings.Add("found project file in root folder. Consider adding a /src folder to contain project files");

        if (context.GitTree.Root.Children.Count < 5) warnings.Add("Root folder looks incomplete. Essential files can be added.");

        var (diagnosis, note) = GetDiagnosis(warnings);
        return Rule.DotnetSolutionStructure(diagnosis, note, $@"
Found {warnings.Count} issues.
<br/>
{string.Join("<br/>", warnings)}
");

        (Diagnosis, string) GetDiagnosis(
            IReadOnlyList<string> e) =>
            e.Any()
                ? (Diagnosis.Warning, $"found {e.Count} potential solution structure issues")
                : (Diagnosis.Info, "did not find any issues");
    }


    private Rule GetRulesetRule(
        AnalysisContext context)
    {
        var nodes = context.GitTree.FilesRecursive(x => x.HasExtension(".ruleset"));
        var details = nodes.Any()
            ? $@"
Found these rulesets:
<br/>
{string.Join("<br/>", nodes.Select(x => x.GetUrl(context)))}
"
            : "";
        var (diagnosis, note) = GetDiagnosis(nodes);
        return Rule.Ruleset(diagnosis, note, details);

        (Diagnosis, string) GetDiagnosis(
            IEnumerable<GitTree.Node> e) =>
            e.Any()
                ? (Diagnosis.Warning, $"found {e.Count()} .ruleset files")
                : (Diagnosis.Info, "did not find ruleset files");
    }

    private Rule GetDotnetTestsRule(
        AnalysisContext context)
    {
        var testProjects = context.GitTree.FilesRecursive(IsTestProject).ToArray();
        var testFiles = context.GitTree.FilesRecursive(IsTestFile).ToArray();
        var (diagnosis, note) = GetDiagnosis(testProjects, testFiles);
        var details = $@"
Detected {testProjects.Length} test projects.
Detected {testFiles.Length} test files.
";
        return Rule.DotnetTests(diagnosis, note, details);

        (Diagnosis, string) GetDiagnosis(
            IEnumerable<GitTree.Node> projects,
            IEnumerable<GitTree.Node> files) =>
            !projects.Any() || !files.Any()
                ? (Diagnosis.Warning, "found issues")
                : (Diagnosis.Info, "found");

        bool IsTestProject(
            GitTree.Node x) =>
            x.PathEndsWith(".csproj") && x.ParentPathEndsWith("test", "tests", "spec", "specs");

        bool IsTestFile(
            GitTree.Node x)
        {
            return x.PathEndsWith(".cs") switch
            {
                true when x.PathEndsWith("test.cs", "tests.cs", "spec.cs", "specs.cs") => true,
                _ => false
            };
        }
    }
}