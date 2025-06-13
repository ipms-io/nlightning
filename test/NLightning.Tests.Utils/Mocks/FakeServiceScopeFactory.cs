using Microsoft.Extensions.DependencyInjection;

namespace NLightning.Tests.Utils.Mocks;

public class FakeServiceScopeFactory : IServiceScopeFactory
{
    public IServiceProvider ServiceProvider { get; }

    public FakeServiceScopeFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IServiceScope CreateScope()
    {
        return new FakeServiceScope(ServiceProvider);
    }
}