using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Payloads;

namespace NLightning.Domain.Protocol.Messages;

/// <summary>
/// Represents a tx_remove_input message.
/// </summary>
/// <remarks>
/// The tx_remove_input message is used to remove an input from the transaction.
/// The message type is 68.
/// </remarks>
/// <param name="payload">The tx_remove_input payload.</param>
public sealed class TxRemoveInputMessage(TxRemoveInputPayload payload) : BaseMessage(MessageTypes.TX_REMOVE_INPUT, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new TxRemoveInputPayload Payload { get => (TxRemoveInputPayload)base.Payload; }
}