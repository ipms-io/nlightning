namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Payloads;

/// <summary>
/// Represents a tx_add_output message.
/// </summary>
/// <remarks>
/// The tx_add_output message is used to add an output to the transaction.
/// The message type is 67.
/// </remarks>
/// <param name="payload">The tx_add_output payload.</param>
public sealed class TxAddOutputMessage(TxAddOutputPayload payload)
    : BaseChannelMessage(MessageTypes.TxAddOutput, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new TxAddOutputPayload Payload { get => (TxAddOutputPayload)base.Payload; }
}