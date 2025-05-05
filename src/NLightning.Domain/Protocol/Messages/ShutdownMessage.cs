namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Payloads;

/// <summary>
/// Represents a shutdown message.
/// </summary>
/// <remarks>
/// The shutdown message is sent by either node to initiate closing, along with the scriptpubkey it wants to be paid to.
/// The message type is 38.
/// </remarks>
/// <param name="payload"></param>
public sealed class ShutdownMessage(ShutdownPayload payload) : BaseChannelMessage(MessageTypes.SHUTDOWN, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new ShutdownPayload Payload { get => (ShutdownPayload)base.Payload; }
}