using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Quality;

internal class GitIgnoreRuleApplicator : IRuleApplicator
{
    public string RuleName => "gitignore";
    public RuleCategory Category => RuleCategory.Quality;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context)
    {
        var node = context.GitTree.SingleFileOrDefault(x => x.PathEquals(".gitignore"));
        var (diagnosis, note, details) = await GetDiagnosis(node);
        return new Rule
        {
            Name = RuleName,
            Category = Category,
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
You can create a .gitignore file in your repository's root directory to tell Git which files and directories to ignore when you make a commit. 
To share the ignore rules with other users who clone the repository, commit the .gitignore file in to your repository.
",
                AboutUrl = "https://docs.github.com/en/get-started/getting-started-with-git/ignoring-files",
                AboutHeader = "about git ignore"
            },
            ResourceName = node?.Item.Path, ResourceUrl = node.GetUrl(context)
        };

        async Task<(Diagnosis, string, string)> GetDiagnosis(
            GitTree.Node? e)
        {
            if (e is null) return (Diagnosis.Error, "missing", "");
            if (context.Repo.PrimaryLanguage is null) return (Diagnosis.Info, "no primary language found, will not analyze", "");

            var (templateName, ignoreList) = await context.RestClient.GetGitIgnoreRules(context.Repo.PrimaryLanguage.Name);
            var ignoredFiles = new List<string>();
            context.GitTree.AnalyzeRecursive(
                x => ignoreList.IsIgnored(x.Item.Path, x.IsTree()),
                (
                    x,
                    _) => ignoredFiles.Add(x.Item.Path));

            var dets = ignoredFiles.Any()
                ? $@"
According to the recommended gitignore rules for the language of the repo, {templateName}.gitignore, 
these files should not exist in the repo. See {Shared.CreateLink("https://github.com/github/gitignore", "Recommended Ignore Files")}
<br/>
Showing first 100 files:
<br/>
<br/>
{string.Join("<br/>", ignoredFiles.Take(100))}"
                : "";

            return ignoredFiles.Any()
                ? (Diagnosis.Warning, "found but contains violations", details: dets)
                : (Diagnosis.Info, "found and without any violations", details: dets);
        }
    }
}