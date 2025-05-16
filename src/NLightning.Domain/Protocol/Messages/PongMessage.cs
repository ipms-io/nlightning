using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Payloads;

namespace NLightning.Domain.Protocol.Messages;

/// <summary>
/// Represents a pong message.
/// </summary>
/// <remarks>
/// The pong message is used to respond to a ping message.
/// The message type is 19.
/// </remarks>
/// <param name="bytesLen">The number of bytes in the pong message.</param>
public sealed class PongMessage(ushort bytesLen) : BaseMessage(MessageTypes.PONG, new PongPayload(bytesLen))
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new PongPayload Payload => (PongPayload)base.Payload;

    internal PongMessage(PongPayload payload) : this(payload.BytesLength)
    {
        base.Payload = payload;
    }
}