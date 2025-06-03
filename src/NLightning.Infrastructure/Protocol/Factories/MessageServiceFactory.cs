using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Protocol.Factories;

using Domain.Protocol.Factories;
using Domain.Protocol.Services;
using Domain.Transport;
using Services;

/// <summary>
/// Factory for creating a message service.
/// </summary>
/// <remarks>
/// This class is used to create a message service in test environments.
/// </remarks>
public sealed class MessageServiceFactory : IMessageServiceFactory
{
    private readonly IMessageSerializer _messageSerializer;

    public MessageServiceFactory(IMessageSerializer messageSerializer)
    {
        _messageSerializer = messageSerializer;
    }

    /// <inheritdoc />
    public IMessageService CreateMessageService(ITransportService transportService)
    {
        return new MessageService(_messageSerializer, transportService);
    }
}