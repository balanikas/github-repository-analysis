# Github Repository Analysis
Service that provides analysis and guidance for public github repositories. 
- Do you already have a public github project you want to check?
- Do you plan to make a project public?

Then this is for you.

#### Status
[![build and test](https://github.com/balanikas/github-repository-analysis/actions/workflows/deploy-to-acr.yaml/badge.svg)](https://github.com/balanikas/github-repository-analysis/actions/workflows/deploy-to-acr.yaml)
[![CodeQL](https://github.com/balanikas/github-repository-analysis/actions/workflows/codeql.yml/badge.svg)](https://github.com/balanikas/github-repository-analysis/actions/workflows/codeql.yml)
![Website](https://img.shields.io/website?down_message=offline&label=service&up_message=online&url=https%3A%2F%2Fgithubrepositoryanalysis.com%2F)

#### Quality
[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=balanikas_github-repository-analysis)

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=balanikas_github-repository-analysis&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=balanikas_github-repository-analysis)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=balanikas_github-repository-analysis&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=balanikas_github-repository-analysis)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=balanikas_github-repository-analysis&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=balanikas_github-repository-analysis)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=balanikas_github-repository-analysis&metric=bugs)](https://sonarcloud.io/summary/new_code?id=balanikas_github-repository-analysis)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=balanikas_github-repository-analysis&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=balanikas_github-repository-analysis)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=balanikas_github-repository-analysis&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=balanikas_github-repository-analysis)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=balanikas_github-repository-analysis&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=balanikas_github-repository-analysis)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=balanikas_github-repository-analysis&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=balanikas_github-repository-analysis)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=balanikas_github-repository-analysis&metric=coverage)](https://sonarcloud.io/summary/new_code?id=balanikas_github-repository-analysis)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=balanikas_github-repository-analysis&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=balanikas_github-repository-analysis)

## How it works
Given a repository, the system will fetch all publicly available data for that repository,
evaluate it based on a set of rules, and present the results together with additional guidance.
The guidance is generated dynamically using the Open AI GPT-3 model.

https://user-images.githubusercontent.com/2317329/207807236-684bf53d-96c3-4c2c-ac73-c6d5389ab1fa.mov

## How to add a new rule
Lets add a rule that detects whether a readme file exists at the repository root.

#### Create the rule class
Create a rule class in an appropriate category. In this case we choose category `Documentation`
```csharp
public class ReadmeRuleApplicator : IRuleApplicator
{
    // annotate questions to be answered by openai gpt-3 model.
    [RuleGuidance] 
    private const string HowToWrite = "Write a short example of a well designed readme file";
    [RuleGuidance] 
    private const string MultipleFiles = "How many readme files can a repository have and why should i have more than one?";
    [RuleGuidance(200, Tone.Motivational, Complexity.Simple)]
    private const string WhatIs = "What is a github readme file and why is it important?";
    
    public string RuleName => "readme";
    public RuleCategory Category => RuleCategory.Documentation;
    public Language Language => Language.None; // set to None since the rule is language agnostic

    public Task<Rule> ApplyAsync(AnalysisContext context) =>
        throw new NotImplementedException();
}
```
#### Implement the rule
```csharp
public async Task<Rule> ApplyAsync(AnalysisContext context)
{
    // apply the rule using the context object
    var node = context.GitTree.FirstFileOrDefault(x => x.PathEquals("readme.md"));
    var (diagnosis, note) = GetDiagnosis(node);
    (Diagnosis, string) GetDiagnosis(GitTree.Node? n) =>
        n is not null
            ? (Diagnosis.Info, "found")
            : (Diagnosis.Error, "missing");
    
    // compose the results
    return Rule.Create(this, diagnostics, new Explanation
    {
        GeneralGuidance = await _gpt3Client.GetCompletions(MultipleFiles, HowToWrite),
        Text = await _gpt3Client.GetCompletion(WhatIs),
        AboutLink = new Link("about readmes",
            "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes"),
        GuidanceLink = diagnostics.Diagnosis == Diagnosis.Error ? context.GetCommunityLink() : null
    });
}
```

#### See the rule in the UI
![custom rule](customrule.png)


For more info and to get started, see [the contributing document](CONTRIBUTING.md)

## About this service
This service is primarily targeted to small or medium sized repos,
that are owned by a single person or a small group of contributors.
It can be very useful to those getting started with open source projects, but also
for more experienced developers who want to do a quality check of their own repos.
A repository can be configured in many ways, and just because this service produces some warnings 
and recommendations based on assumptions and standard practices, doesn't mean that the repository
is in a definitive bad shape. 

The analysis is presented as a set of detections, where each detection is either
- `Okay` - looks good and no action needed 
- `Can be improved` - an action can be taken for improvement
- `Warning` - strongly recommended to address this

This service currently has extra checks for csharp repositories, but any repository should work for general rules.
For feedback, please create a new issue at https://github.com/balanikas/github-repository-analysis/issues
