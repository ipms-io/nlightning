namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Payloads;

/// <summary>
/// Represents a update_fulfill_htlc message.
/// </summary>
/// <remarks>
/// The update_fulfill_htlc message is sent to let the peer know that the htlc was fulfiled
/// The message type is 130.
/// </remarks>
/// <param name="payload"></param>
public sealed class UpdateFulfillHtlcMessage(UpdateFulfillHtlcPayload payload)
    : BaseChannelMessage(MessageTypes.UpdateFufillHtlc, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new UpdateFulfillHtlcPayload Payload { get => (UpdateFulfillHtlcPayload)base.Payload; }
}