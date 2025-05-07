namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Payloads;

/// <summary>
/// Represents a pong message.
/// </summary>
/// <remarks>
/// The pong message is used to respond to a ping message.
/// The message type is 19.
/// </remarks>
/// <param name="bytesLen">The number of bytes in the pong message.</param>
public sealed class PongMessage(ushort bytesLen) : BaseMessage(MessageTypes.Pong, new PongPayload(bytesLen))
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