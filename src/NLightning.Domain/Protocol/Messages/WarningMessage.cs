using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Payloads;

namespace NLightning.Domain.Protocol.Messages;

/// <summary>
/// Represents a warning message.
/// </summary>
/// <remarks>
/// A warning message is used to communicate a warning to the other party.
/// The message type is 1.
/// </remarks>
/// <param name="payload">The warning payload. <see cref="ErrorPayload"/></param>
public sealed class WarningMessage(ErrorPayload payload) : BaseMessage(MessageTypes.WARNING, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new ErrorPayload Payload { get => (ErrorPayload)base.Payload; }
}