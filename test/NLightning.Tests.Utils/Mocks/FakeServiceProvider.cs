using Microsoft.Extensions.DependencyInjection;

namespace NLightning.Tests.Utils.Mocks;

public class FakeServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> _services = [];

    public FakeServiceProvider()
    {
        _services.Add(typeof(IServiceScopeFactory), new FakeServiceScopeFactory(this));
    }

    public object? GetService(Type serviceType)
    {
        return _services[serviceType] ?? throw new Exception("You should add the service first.");
    }

    public void AddService<T>(T serviceType, object service)
    {
        _services.Add(serviceType as Type ?? throw new Exception("Send Interface for type"), service);
    }
}