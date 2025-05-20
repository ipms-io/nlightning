namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Payloads;

/// <summary>
/// Represents an update_fail_malformed_htlc message.
/// </summary>
/// <remarks>
/// The update_fail_malformed_htlc message is sent to let the peer know that the htlc is malformed
/// The message type is 135.
/// </remarks>
/// <param name="payload"></param>
public sealed class UpdateFailMalformedHtlcMessage(UpdateFailMalformedHtlcPayload payload)
    : BaseMessage(MessageTypes.UPDATE_FAIL_MALFORMED_HTLC, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new UpdateFailMalformedHtlcPayload Payload { get => (UpdateFailMalformedHtlcPayload)base.Payload; }
}