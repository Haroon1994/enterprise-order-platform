using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace EnterpriseOrderPlatform.ArchitectureTests;

public class IdentityModuleRulesTests
{
    private const string IdentityRoot = "EnterpriseOrderPlatform.Modules.Identity";
    private const string DomainNs = IdentityRoot + ".Domain";
    private const string ApplicationNs = IdentityRoot + ".Application";
    private const string InfrastructureNs = IdentityRoot + ".Infrastructure";

    private static readonly Assembly Identity =
        typeof(EnterpriseOrderPlatform.Modules.Identity.AssemblyMarker).Assembly;

    [Fact]
    public void IdentityDomain_ContainsTypes()
    {
        // Guard: a rule over zero types would pass without checking anything.
        var types = Types.InAssembly(Identity).That().ResideInNamespace(DomainNs).GetTypes();

        Assert.NotEmpty(types);
    }

    [Fact]
    public void IdentityDomain_DoesNotDependOnApplicationOrInfrastructure()
    {
        var result = Types.InAssembly(Identity)
            .That().ResideInNamespace(DomainNs)
            .ShouldNot().HaveDependencyOnAny(ApplicationNs, InfrastructureNs)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void IdentityApplication_DoesNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(Identity)
            .That().ResideInNamespace(ApplicationNs)
            .ShouldNot().HaveDependencyOnAny(InfrastructureNs)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(NetArchTest.Rules.TestResult result) =>
        result.FailingTypes is null
            ? string.Empty
            : "Forbidden dependencies in: " + string.Join(", ", result.FailingTypes.Select(t => t.FullName));
}
