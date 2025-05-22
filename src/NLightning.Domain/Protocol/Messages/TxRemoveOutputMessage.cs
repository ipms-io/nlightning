namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Payloads;

/// <summary>
/// Represents a tx_remove_output message.
/// </summary>
/// <remarks>
/// The tx_remove_output message is used to remove an output from the transaction.
/// The message type is 69.
/// </remarks>
/// <param name="payload">The tx_remove_output payload.</param>
public sealed class TxRemoveOutputMessage(TxRemoveOutputPayload payload)
    : BaseChannelMessage(MessageTypes.TxRemoveOutput, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new TxRemoveOutputPayload Payload { get => (TxRemoveOutputPayload)base.Payload; }
}