using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Payloads;

namespace NLightning.Domain.Protocol.Messages;

/// <summary>
/// Represents a update_fail_htlc message.
/// </summary>
/// <remarks>
/// The update_fail_htlc message is sent to let the peer know that the htlc has failed
/// The message type is 131.
/// </remarks>
/// <param name="payload"></param>
public sealed class UpdateFailHtlcMessage(UpdateFailHtlcPayload payload) : BaseMessage(MessageTypes.UPDATE_FAIL_HTLC, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new UpdateFailHtlcPayload Payload { get => (UpdateFailHtlcPayload)base.Payload; }
}