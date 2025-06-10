namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Interfaces;
using Models;
using Payloads;

public abstract class BaseChannelMessage : BaseMessage, IChannelMessage
{
    /// <inheritdoc />
    public new virtual IChannelMessagePayload Payload { get; protected set; }

    public BaseChannelMessage(MessageTypes type, IChannelMessagePayload payload, TlvStream? extension = null)
        : base(type, payload, extension)
    {
        Payload = payload;
    }

    internal BaseChannelMessage(MessageTypes type) : base(type)
    {
        Payload = new PlaceholderPayload();
    }
}