// using RepositoryAnalysis.Model;
//
// namespace RepositoryAnalysis.Internal.Rules.Documentation;
//
// public class HomePageRuleApplicator : IRuleApplicator
// {
//     public string RuleName => "homepage";
//     public RuleCategory Category => RuleCategory.Documentation;
//     public Language Language => Language.None;
//
//     public async Task<Rule> ApplyAsync(
//         AnalysisContext context) => await Task.FromResult(Apply(context));
//
//     private Rule Apply(
//         AnalysisContext context)
//     {
//         var (diagnosis, note) = GetDiagnosis();
//         //todo: check head request if url exists
//         return new Rule
//         {
//             Name = RuleName,
//             Category = Category,
//             Note = note,
//             Diagnosis = diagnosis,
//             Explanation = new Explanation
//             {
//                 Details = null,
//                 Text = @"
// A repository homepage url helps users to get more information about the project. 
// It can be edited in the About section.",
//                 GuidanceUrl = "https://docs.github.com/en/get-started/quickstart/create-a-repo",
//                 GuidanceHeader = "this guide on how to create a repository"
//             },
//             ResourceName = context.Repo.HomepageUrl?.ToString(), ResourceUrl = context.Repo.HomepageUrl?.ToString()
//         };
//
//         (Diagnosis, string) GetDiagnosis() =>
//             !string.IsNullOrEmpty(context.Repo.HomepageUrl?.ToString())
//                 ? (Diagnosis.Info, "found homepage")
//                 : (Diagnosis.Warning, "missing");
//     }
// }

