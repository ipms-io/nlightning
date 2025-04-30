using NLightning.Domain.Protocol.Messages.Interfaces;
using NLightning.Domain.Protocol.Payloads.Interfaces;

namespace NLightning.Domain.Protocol.Messages;

using Common.Interfaces;
using Models;
using Payloads;

public abstract class BaseChannelMessage : BaseMessage, IChannelMessage
{
    /// <inheritdoc />
    public new virtual IChannelMessagePayload Payload { get; protected set; }

    public BaseChannelMessage(ushort type, IChannelMessagePayload payload, TlvStream? extension = null)
        : base(type, payload, extension)
    {
        Payload = payload;
    }

    internal BaseChannelMessage(ushort type) : base(type)
    {
        Payload = new PlaceholderPayload();
    }
}