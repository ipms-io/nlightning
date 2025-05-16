using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Payloads;

namespace NLightning.Domain.Protocol.Messages;

/// <summary>
/// Represents an tx_add_input message.
/// </summary>
/// <remarks>
/// The tx_add_input message is used to add an input to the transaction.
/// The message type is 66.
/// </remarks>
/// <param name="payload">The tx_add_input payload.</param>
public sealed class TxAddInputMessage(TxAddInputPayload payload) : BaseMessage(MessageTypes.TX_ADD_INPUT, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new TxAddInputPayload Payload { get => (TxAddInputPayload)base.Payload; }
}