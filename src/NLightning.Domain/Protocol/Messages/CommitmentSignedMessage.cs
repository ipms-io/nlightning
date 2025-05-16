namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Payloads;

/// <summary>
/// Represents a commitment_signed message.
/// </summary>
/// <remarks>
/// The commitment_signed message is sent when a node has changes to the remote commitment
/// The message type is 132.
/// </remarks>
/// <param name="payload"></param>
public sealed class CommitmentSignedMessage(CommitmentSignedPayload payload) : BaseMessage(MessageTypes.COMMITMENT_SIGNED, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new CommitmentSignedPayload Payload { get => (CommitmentSignedPayload)base.Payload; }
}