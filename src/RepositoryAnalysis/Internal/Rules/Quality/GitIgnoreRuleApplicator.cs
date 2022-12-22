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

            var fileContent = await context.GetFile(".gitignore");
            var ignore = new Ignore.Ignore();
            ignore.Add(fileContent.Split("\n"));

            var ignoredFiles = new List<string>();
            context.GitTree.AnalyzeRecursive(
                x => ignore.IsIgnored(x.Item.Path),
                (
                    x,
                    _) => ignoredFiles.Add(x.Item.Path));

            var visualCount = ignoredFiles.Count > 50 ? 50 : ignoredFiles.Count;
            var details = ignoredFiles.Any()
                ? $@"
According to the gitignore rules at the root of this repo,
these files should not exist in the repo. See {Shared.CreateLink("https://github.com/github/gitignore", "Recommended Ignore Files")}
<br/>
Showing first {visualCount} files:
<br/>
{string.Join("<br/>", ignoredFiles.Take(visualCount))}"
                : "";

            return ignoredFiles.Any()
                ? (Diagnosis.Warning, "found but contains violations", details)
                : (Diagnosis.Info, "found and without any violations", details);
        }
    }
}