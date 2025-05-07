namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Models;
using Payloads;
using Tlv;

/// <summary>
/// Represents a update_add_htlc message.
/// </summary>
/// <remarks>
/// The update_add_htlc message offers a new htlc to the peer.
/// The message type is 128.
/// </remarks>
public sealed class UpdateAddHtlcMessage : BaseChannelMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new UpdateAddHtlcPayload Payload { get => (UpdateAddHtlcPayload)base.Payload; }

    public BlindedPathTlv? BlindedPathTlv { get; }

    public UpdateAddHtlcMessage(UpdateAddHtlcPayload payload, BlindedPathTlv? blindedPathTlv = null)
        : base(MessageTypes.UpdateAddHtlc, payload)
    {
        BlindedPathTlv = blindedPathTlv;

        if (BlindedPathTlv is not null)
        {
            Extension = new TlvStream();
            Extension.Add(BlindedPathTlv);
        }
    }
}