namespace NLightning.Domain.Protocol.Messages;

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
    public ushort Type { get; }

    /// <inheritdoc />
    public virtual IMessagePayload Payload { get; protected init; }

    /// <inheritdoc />
    public TlvStream? Extension { get; protected init; }

    protected BaseMessage(ushort type, IMessagePayload payload, TlvStream? extension = null)
    {
        Type = type;
        Payload = payload;
        Extension = extension;
    }
    protected internal BaseMessage(ushort type)
    {
        Type = type;
        Payload = new PlaceholderPayload();
    }
}