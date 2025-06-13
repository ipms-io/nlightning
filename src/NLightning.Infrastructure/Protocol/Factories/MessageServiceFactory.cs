using Microsoft.Extensions.Logging;

namespace NLightning.Infrastructure.Protocol.Factories;

using Domain.Protocol.Interfaces;
using Domain.Serialization.Interfaces;
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
    private readonly ILoggerFactory _loggerFactory;

    public MessageServiceFactory(IMessageSerializer messageSerializer, ILoggerFactory loggerFactory)
    {
        _messageSerializer = messageSerializer;
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    public IMessageService CreateMessageService(ITransportService transportService)
    {
        return new MessageService(_loggerFactory.CreateLogger<IMessageService>(), _messageSerializer, transportService);
    }
}