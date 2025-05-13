namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Payloads;

/// <summary>
/// Represents an stfu message.
/// </summary>
/// <remarks>
/// The stfu message means SomeThing Fundamental is Underway, so we kindly ask the other node to STFU because we have
/// something important to say
/// The message type is 2.
/// </remarks>
/// <param name="payload"></param>
public sealed class StfuMessage(StfuPayload payload) : BaseMessage(MessageTypes.STFU, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new StfuPayload Payload { get => (StfuPayload)base.Payload; }
}