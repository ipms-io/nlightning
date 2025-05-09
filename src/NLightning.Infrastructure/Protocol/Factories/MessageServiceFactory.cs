using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.Transport.Interfaces;
using NLightning.Infrastructure.Protocol.Services;

namespace NLightning.Bolts.BOLT1.Factories;

using Common.Interfaces;

/// <summary>
/// Factory for creating a message service.
/// </summary>
/// <remarks>
/// This class is used to create a message service in test environments.
/// </remarks>
public sealed class MessageServiceFactory : IMessageServiceFactory
{
    private readonly IMessageFactory _messageFactory;

    public MessageServiceFactory(IMessageFactory messageFactory)
    {
        _messageFactory = messageFactory;
    }

    /// <inheritdoc />
    public IMessageService CreateMessageService(ITransportService transportService)
    {
        return new MessageService(_messageFactory, transportService);
    }
}