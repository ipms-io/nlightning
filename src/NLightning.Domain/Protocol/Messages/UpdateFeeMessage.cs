using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Payloads;

namespace NLightning.Domain.Protocol.Messages;

/// <summary>
/// Represents a update_fee message.
/// </summary>
/// <remarks>
/// The update_fee message is sent by the node which is paying the Bitcoin fee.
/// The message type is 134.
/// </remarks>
/// <param name="payload"></param>
public sealed class UpdateFeeMessage(UpdateFeePayload payload) : BaseMessage(MessageTypes.UPDATE_FEE, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new UpdateFeePayload Payload { get => (UpdateFeePayload)base.Payload; }
}