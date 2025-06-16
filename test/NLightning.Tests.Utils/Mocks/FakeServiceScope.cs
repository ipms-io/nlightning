using Microsoft.Extensions.DependencyInjection;

namespace NLightning.Tests.Utils.Mocks;

public class FakeServiceScope : IServiceScope
{
    public IServiceProvider ServiceProvider { get; }

    public FakeServiceScope(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public void Dispose()
    {
        // NoOp - this is a fake scope for testing purposes
    }
}