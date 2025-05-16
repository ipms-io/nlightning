using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Payloads;

namespace NLightning.Domain.Protocol.Messages;

/// <summary>
/// Represents a tx_abort message.
/// </summary>
/// <remarks>
/// The tx_abort message allows for the cancellation of an in-progress negotiation.
/// The message type is 74.
/// </remarks>
/// <param name="payload">The tx_abort payload.</param>
public sealed class TxAbortMessage(TxAbortPayload payload) : BaseMessage(MessageTypes.TX_ABORT, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new TxAbortPayload Payload { get => (TxAbortPayload)base.Payload; }
}