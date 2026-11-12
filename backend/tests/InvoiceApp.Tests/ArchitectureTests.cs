using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Domain_DoesNotReferenceApi()
    {
        var domainAssembly = typeof(Invoice).Assembly;

        Assert.DoesNotContain(
            domainAssembly.GetReferencedAssemblies(),
            reference => string.Equals(reference.Name, "InvoiceApp.Api", StringComparison.Ordinal));
    }
}
