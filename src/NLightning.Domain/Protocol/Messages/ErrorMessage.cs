namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Payloads;

/// <summary>
/// Represents an error message.
/// </summary>
/// <remarks>
/// An error message is used to communicate an error to the other party.
/// The message type is 17.
/// </remarks>
/// <param name="payload">The error payload.</param>
public sealed class ErrorMessage(ErrorPayload payload) : BaseMessage(MessageTypes.Error, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new ErrorPayload Payload { get => (ErrorPayload)base.Payload; }
}