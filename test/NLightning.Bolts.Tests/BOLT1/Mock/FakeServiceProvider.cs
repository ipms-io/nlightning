namespace NLightning.Bolts.Tests.BOLT1.Mock;

public class FakeServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> _services = new();

    public object? GetService(Type serviceType)
    {
        return _services[serviceType] ?? throw new Exception("You should add the service first.");
    }

    public void AddService<T>(T serviceType, object service)
    {
        _services.Add(serviceType as Type ?? throw new Exception("Send Interface for type"), service);
    }
}