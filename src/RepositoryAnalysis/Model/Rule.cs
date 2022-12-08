using System.Text;
using RepositoryAnalysis.Internal;

namespace RepositoryAnalysis.Model;

public  record Rule()
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string Name { get; init; }
    public string Note { get; init; } = "";
    public string? ResourceName { get; init; }
    public string? ResourceUrl { get; init; }
    public required Explanation Explanation { get; init; }
    public Diagnosis Diagnosis { get; private init; } = Diagnosis.Info;

    protected virtual bool PrintMembers(
        StringBuilder builder)
    {
        builder.Append($"Id = {Id}");
        return true;
    }
         

    public virtual bool Equals(
        Rule? other) =>
        other is not null && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();


    public static Rule DotnetSolutionStructure(
        Diagnosis diagnosis,
        string note,
        string? details) =>
        new()
        {
            Diagnosis = diagnosis,
            Note = note,
            Name = "Dotnet solution structure",
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
It is good practice to follow standard solution structure conventions. ",
                AboutUrl = null,
                AboutHeader = null,
                GuidanceUrl = null,
                GuidanceHeader = null
            }
        };

    public static Rule CitationFile(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Diagnosis = diagnosis,
            Note = note,
            Name = "Citation file",
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
You can add a CITATION file to your repository to help users correctly cite your software.
",
                AboutUrl =
                    "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-citation-files",
                AboutHeader = "about citation files",
                GuidanceUrl = null,
                GuidanceHeader = null
            }
        };

    public static Rule SupportFile(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Diagnosis = diagnosis,
            Note = note,
            Name = "Support file",
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
You can create a SUPPORT file to let people know about ways to get help with your project.
To direct people to specific support resources, you can add a SUPPORT file to your repository's root, docs, or .github folder. 
When someone creates an issue in your repository, they will see a link to your project's SUPPORT file.
",
                AboutUrl = "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/adding-support-resources-to-your-project",
                AboutHeader = "about support files",
                GuidanceUrl =
                    "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/adding-support-resources-to-your-project",
                GuidanceHeader = "how to add a support file"
            }
        };

    public static Rule PullRequests(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Name = "Pull requests",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
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

    public static Rule Issues(
        Diagnosis diagnosis,
        string note,
        string? details) =>
        new()
        {
            Name = "Issues",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
Issues let you track your work on GitHub, where development happens.
You may wish to turn issues off for your repository if you do not accept contributions or bug reports.
",
                AboutUrl = "https://docs.github.com/en/issues/tracking-your-work-with-issues/about-issues",
                AboutHeader = "about issues"
            }
        };

    public static Rule Discussions(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Name = "Discussions",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
Use discussions to ask and answer questions, share information, make announcements, and conduct or participate in a conversation about a project on GitHub.
With GitHub Discussions, the community for your project can create and participate in conversations within the project's repository or organization. 
Discussions empower a project's maintainers, contributors, and visitors to gather and accomplish the following goals in a central location, without third-party tools.",
                AboutUrl = "https://docs.github.com/en/discussions/collaborating-with-your-community-using-discussions/about-discussions",
                AboutHeader = "about discussions"
            }
        };

    public static Rule CodeOwners(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Name = "Code owners",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
You can use a CODEOWNERS file to define individuals or teams that are responsible for code in a repository.",
                AboutUrl =
                    "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-code-owners",
                AboutHeader = "about code owners"
            }
        };

    public static Rule CodeOfConduct(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Name = "Code of conduct",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
Adopt a code of conduct to define community standards, signal a welcoming and inclusive project, and outline procedures for handling abuse.
A code of conduct defines standards for how to engage in a community. It signals an inclusive environment that respects all contributions. 
It also outlines procedures for addressing problems between members of your project's community. ",
                AboutUrl = "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/adding-a-code-of-conduct-to-your-project",
                AboutHeader = "about code of conduct"
            }
        };

    public static Rule Contributing(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Name = "Contributing",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
To help your project contributors do good work, you can add a file with contribution guidelines to your project repository's root, docs, or .github folder. 
When someone opens a pull request or creates an issue, they will see a link to that file. The link to the contributing guidelines also appears on your repository's contribute page.",
                AboutUrl =
                    "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/setting-guidelines-for-repository-contributors",
                AboutHeader = "about contributing guidelines"
            }
        };

    public static Rule Topics(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Name = "Topics",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
To help other people find and contribute to your project, you can add topics to your repository related to your project's intended purpose, subject area, affinity groups, or other important qualities.",
                AboutUrl =
                    "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/classifying-your-repository-with-topics",
                AboutHeader = "about topics",
                GuidanceUrl = "https://docs.github",
                GuidanceHeader = "how to work with topics"
            }
        };

    public static Rule ChangeLog(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        //todo: look in github releases too
        new()
        {
            Name = "Change log",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = $@"
A changelog is a kind of summary of all your changes. 
It should be easy to understand both by the users using your project and the developers working on it.
Adding a CHANGELOG.md file in the repo root is a good start.
<br/>
Note: this currently only look for related files in the repo root, and does not look in 
{Shared.CreateLink("https://help.github.com/articles/creating-releases/", "Github Releases")}"
            }
        };

    public static Rule HomePage(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Name = "Homepage",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
A repository homepage url helps users to get more information about the project. 
It can be edited in the About section.",
                GuidanceUrl = "https://docs.github.com/en/get-started/quickstart/create-a-repo",
                GuidanceHeader = "this guide on how to create a repository"
            }
        };

    public static Rule Description(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Name = "Description/About",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
A repository description helps users to understand what the repository is about.
It can be edited in the About section.",
                GuidanceUrl = "https://docs.github.com/en/get-started/quickstart/create-a-repo",
                GuidanceHeader = "this guide on how to create a repository"
            }
        };

    public static Rule Readme(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Name = "Readme",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
A repository should contain a readme file, to tell other people why your project is useful, what they can do with your project, and how they can use it.",
                AboutUrl = "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes",
                AboutHeader = "about readmes"
            }
        };


    public static Rule Ruleset(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Name = "Ruleset",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
Ruleset configuration files for static code analysis are being deprecated in favor of more modern tools, 
like EditorConfig and dotnet analyzers.",
                AboutUrl = "https://learn.microsoft.com/en-us/visualstudio/code-quality/analyzers-faq?view=vs-2022",
                AboutHeader = "about dotnet analyzers"
            }
        };

    public static Rule DotnetTests(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Name = "Tests",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
Tests increase the quality of software. 
",
                AboutUrl = "https://learn.microsoft.com/en-us/dotnet/core/testing/",
                AboutHeader = "testing in dotnet"
            }
        };

    public static Rule License(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Name = "License",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
Public repositories on GitHub are often used to share open source software. 
For your repository to truly be open source, you'll need to license it so that others are free to use, change, and distribute the software.",
                AboutUrl =
                    "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository",
                AboutHeader = "about open source licensing",
                GuidanceUrl =
                    "https://choosealicense.com/",
                GuidanceHeader = "how to choose a license"
            }
        };

    public static Rule GitIgnore(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Name = "Git ignore",
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
            }
        };

    public static Rule LargeFiles(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Name = "Large files",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
Large files contained in a repository might be a sign of unoptimized repository. 
",
                AboutUrl = "https://docs.github.com/en/repositories/working-with-files/managing-large-files/about-large-files-on-github",
                AboutHeader = "about large files"
            }
        };

    public static Rule EditorConfig(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Name = "Editorconfig",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
EditorConfig helps maintain consistent coding styles for multiple developers working on the same project across various editors and IDEs. 
The EditorConfig project consists of a file format for defining coding styles and a collection of text editor plugins that enable editors to read the file format and adhere to defined styles. 
EditorConfig files are easily readable and they work nicely with version control systems.",
                AboutUrl = "https://editorconfig.org/",
                AboutHeader = "about editor config"
            }
        };

    public static Rule DockerFile(
        Diagnosis diagnosis,
        string note,
        string? details = null) =>
        new()
        {
            Name = "Dockerfile",
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
Before the docker CLI sends the context to the docker daemon, it looks for a file named .dockerignore in the root directory 
of the context. If this file exists, the CLI modifies the context to exclude files and directories that match patterns in it. 
This helps to avoid unnecessarily sending large or sensitive files and directories to the daemon and potentially adding them 
to images using ADD or COPY.",
                AboutUrl = "https://docs.docker.com/develop/develop-images/dockerfile_best-practices/",
                AboutHeader = "about Dockerfile"
            }
        };
}