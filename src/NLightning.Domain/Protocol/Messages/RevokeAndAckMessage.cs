using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Payloads;

namespace NLightning.Domain.Protocol.Messages;

/// <summary>
/// Represents a revoke_and_ack message.
/// </summary>
/// <remarks>
/// The revoke_and_ack message is used as a reply to the commitment_signed message.
/// The message type is 133.
/// </remarks>
/// <param name="payload"></param>
public sealed class RevokeAndAckMessage(RevokeAndAckPayload payload) : BaseMessage(MessageTypes.REVOKE_AND_ACK, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new RevokeAndAckPayload Payload { get => (RevokeAndAckPayload)base.Payload; }
}