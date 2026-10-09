using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace EnterpriseOrderPlatform.ArchitectureTests;

public class DependencyRuleTests
{
    private const string ApplicationNs = "EnterpriseOrderPlatform.Application";
    private const string InfrastructureNs = "EnterpriseOrderPlatform.Infrastructure";
    private const string ApiNs = "EnterpriseOrderPlatform.Api";

    private static readonly Assembly Domain = typeof(EnterpriseOrderPlatform.Domain.AssemblyMarker).Assembly;
    private static readonly Assembly Application = typeof(EnterpriseOrderPlatform.Application.AssemblyMarker).Assembly;
    private static readonly Assembly Infrastructure = typeof(EnterpriseOrderPlatform.Infrastructure.AssemblyMarker).Assembly;

    [Fact]
    public void Domain_DoesNotDependOnOtherLayers()
    {
        var result = Types.InAssembly(Domain)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNs, InfrastructureNs, ApiNs)
            .GetResult();

        Assert.True(result.IsSuccessful, FailingTypes(result));
    }

    [Fact]
    public void Application_DoesNotDependOnInfrastructureOrApi()
    {
        var result = Types.InAssembly(Application)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNs, ApiNs)
            .GetResult();

        Assert.True(result.IsSuccessful, FailingTypes(result));
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnApi()
    {
        var result = Types.InAssembly(Infrastructure)
            .ShouldNot()
            .HaveDependencyOnAny(ApiNs)
            .GetResult();

        Assert.True(result.IsSuccessful, FailingTypes(result));
    }

    private static string FailingTypes(NetArchTest.Rules.TestResult result) =>
        result.FailingTypes is null
            ? string.Empty
            : "Forbidden dependencies in: " + string.Join(", ", result.FailingTypes.Select(t => t.FullName));
}
