namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Payloads;
using Tlvs;
using ValueObjects;

/// <summary>
/// Represents an init message.
/// </summary>
/// <remarks>
/// The init message is used to communicate the features of the node.
/// The message type is 16.
/// </remarks>
public sealed class InitMessage : BaseMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new InitPayload Payload { get => (InitPayload)base.Payload; }

    public NetworksTlv? NetworksTlv { get; }

    public InitMessage(InitPayload payload, NetworksTlv? networksTlv = null) : base(MessageTypes.INIT, payload)
    {
        NetworksTlv = networksTlv;

        if (networksTlv is not null)
        {
            Extension = new TlvStream();
            Extension.Add(networksTlv);
        }
    }
}