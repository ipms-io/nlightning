namespace NLightning.Bolts.BOLT1.Factories;

using BOLT8.Interfaces;
using Interfaces;
using Services;

/// <summary>
/// Factory for creating a message service.
/// </summary>
/// <remarks>
/// This class is used to create a message service in test environments.
/// </remarks>
internal sealed class MessageServiceFactory : IMessageServiceFactory
{
    /// <inheritdoc />
    public IMessageService CreateMessageService(ITransportService transportService)
    {
        return new MessageService(transportService);
    }
}