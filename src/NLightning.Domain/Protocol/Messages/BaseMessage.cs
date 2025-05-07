namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Interfaces;
using Models;
using Payloads;
using Payloads.Interfaces;

/// <summary>
/// Base class for a message.
/// </summary>
public abstract class BaseMessage : IMessage
{
    /// <inheritdoc />
    public MessageTypes Type { get; }

    /// <inheritdoc />
    public virtual IMessagePayload Payload { get; protected init; }

    /// <inheritdoc />
    public TlvStream? Extension { get; protected init; }

    protected BaseMessage(MessageTypes type, IMessagePayload payload, TlvStream? extension = null)
    {
        Type = type;
        Payload = payload;
        Extension = extension;
    }
    protected internal BaseMessage(MessageTypes type)
    {
        Type = type;
        Payload = new PlaceholderPayload();
    }
}