namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Payloads;

/// <summary>
/// Represents a ping message.
/// </summary>
/// <remarks>
/// The ping message is used to check if the other party is still alive.
/// The message type is 18.
/// </remarks>
public sealed class PingMessage() : BaseMessage(MessageTypes.PING, new PingPayload())
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new PingPayload Payload => (PingPayload)base.Payload;

    internal PingMessage(PingPayload payload) : this()
    {
        base.Payload = payload;
    }
}