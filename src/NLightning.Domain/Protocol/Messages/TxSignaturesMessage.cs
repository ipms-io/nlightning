namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Payloads;

/// <summary>
/// Represents a tx_signatures message.
/// </summary>
/// <remarks>
/// The tx_signatures message signals the provision of transaction signatures.
/// The message type is 71.
/// </remarks>
/// <param name="payload">The tx_signatures payload.</param>
public sealed class TxSignaturesMessage(TxSignaturesPayload payload)
    : BaseChannelMessage(MessageTypes.TxSignatures, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new TxSignaturesPayload Payload { get => (TxSignaturesPayload)base.Payload; }
}