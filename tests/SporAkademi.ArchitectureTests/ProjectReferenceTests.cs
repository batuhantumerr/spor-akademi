using System.Xml.Linq;

namespace SporAkademi.ArchitectureTests;

// ADR-0001 dependency rule, checked on the .csproj files. The compiler drops project references whose types are
// never used, so LayerDependencyTests cannot see a wrong reference until code uses it; this test can.
public sealed class ProjectReferenceTests
{
    private const string SolutionFileName = "SporAkademi.slnx";

    private static readonly Dictionary<string, string[]> _rules = new()
    {
        ["SporAkademi.Domain"] = [],
        ["SporAkademi.Application"] = ["SporAkademi.Domain"],
        ["SporAkademi.Infrastructure"] = ["SporAkademi.Application"],
        ["SporAkademi.Api"] = ["SporAkademi.Application", "SporAkademi.Infrastructure"],
    };

    public static TheoryData<string, string[]> AllowedReferences
    {
        get
        {
            var data = new TheoryData<string, string[]>();
            foreach (var (project, references) in _rules)
            {
                data.Add(project, references);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(AllowedReferences))]
    public void SourceProject_ReferencesOnlyAllowedProjects(string project, string[] expectedReferences)
    {
        var projectFile = Path.Combine(SourceDirectory(), project, $"{project}.csproj");

        var references = XDocument.Load(projectFile)
            .Descendants("ProjectReference")
            .Select(reference => Path.GetFileNameWithoutExtension(reference.Attribute("Include")!.Value));

        references.ShouldBe(expectedReferences, ignoreOrder: true);
    }

    [Fact]
    public void SourceDirectory_ContainsOnlyProjectsWithAReferenceRule()
    {
        var projects = Directory.GetFiles(SourceDirectory(), "*.csproj", SearchOption.AllDirectories)
            .Select(Path.GetFileNameWithoutExtension);

        projects.ShouldBe(_rules.Keys, ignoreOrder: true);
    }

    private static string SourceDirectory() => Path.Combine(RepositoryRoot(), "src");

    // Walks up from the test output folder (e.g. tests/.../bin/Debug/net10.0) to the folder holding the solution.
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException($"{SolutionFileName} was not found above {AppContext.BaseDirectory}.");
    }
}
