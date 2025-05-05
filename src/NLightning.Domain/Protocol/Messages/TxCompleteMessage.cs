namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Payloads;

/// <summary>
/// Represents a tx_complete message.
/// </summary>
/// <remarks>
/// The tx_complete message signals the conclusion of a peer's transaction contributions.
/// The message type is 70.
/// </remarks>
/// <param name="payload">The tx_complete payload.</param>
public sealed class TxCompleteMessage(TxCompletePayload payload) : BaseChannelMessage(MessageTypes.TX_COMPLETE, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new TxCompletePayload Payload { get => (TxCompletePayload)base.Payload; }
}