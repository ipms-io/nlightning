using NLightning.Domain.Protocol.Models;
using NLightning.Infrastructure.Protocol.Models;

namespace NLightning.Domain.Protocol.Messages;

using Interfaces;
using Payloads;
using ValueObjects;

/// <summary>
/// Base class for a message.
/// </summary>
public abstract class BaseMessage : IMessage
{
    /// <inheritdoc />
    public ushort Type { get; protected set; }

    /// <inheritdoc />
    public virtual IMessagePayload Payload { get; protected set; }

    /// <inheritdoc />
    public TlvStream? Extension { get; protected set; }

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