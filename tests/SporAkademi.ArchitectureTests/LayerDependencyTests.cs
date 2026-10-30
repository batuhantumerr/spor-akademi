using System.Reflection;
using NetArchTest.Rules;
using TestResult = NetArchTest.Rules.TestResult;

namespace SporAkademi.ArchitectureTests;

// ADR-0001 dependency rule, checked on compiled types: Api -> Application, Infrastructure;
// Infrastructure -> Application; Application -> Domain; Domain -> nothing.
public sealed class LayerDependencyTests
{
    private const string ApplicationNamespace = "SporAkademi.Application";
    private const string InfrastructureNamespace = "SporAkademi.Infrastructure";
    private const string ApiNamespace = "SporAkademi.Api";

    private static readonly Assembly _domainAssembly = typeof(Domain.AssemblyMarker).Assembly;
    private static readonly Assembly _applicationAssembly = typeof(Application.AssemblyMarker).Assembly;
    private static readonly Assembly _infrastructureAssembly = typeof(Infrastructure.AssemblyMarker).Assembly;

    [Fact]
    public void Domain_DoesNotDependOnOtherLayers()
    {
        var result = Types.InAssembly(_domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(FailureMessage(result));
    }

    [Fact]
    public void Domain_DoesNotDependOnFrameworks()
    {
        var result = Types.InAssembly(_domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(FailureMessage(result));
    }

    [Fact]
    public void Application_DoesNotDependOnInfrastructureOrApi()
    {
        var result = Types.InAssembly(_applicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(FailureMessage(result));
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnApi()
    {
        var result = Types.InAssembly(_infrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(FailureMessage(result));
    }

    private static string FailureMessage(TestResult result) =>
        $"Types breaking the layer dependency rule: {string.Join(", ", result.FailingTypeNames ?? [])}";
}
