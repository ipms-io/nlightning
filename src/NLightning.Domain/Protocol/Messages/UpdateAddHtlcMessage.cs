using System.Runtime.Serialization;
using NLightning.Common.TLVs;
using NLightning.Domain.Protocol.Payloads;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Messages;

/// <summary>
/// Represents a update_add_htlc message.
/// </summary>
/// <remarks>
/// The update_add_htlc message offers a new htlc to the peer.
/// The message type is 128.
/// </remarks>
public sealed class UpdateAddHtlcMessage : BaseMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new UpdateAddHtlcPayload Payload { get => (UpdateAddHtlcPayload)base.Payload; }

    public BlindedPathTlv? BlindedPathTlv { get; }

    public UpdateAddHtlcMessage(UpdateAddHtlcPayload payload, BlindedPathTlv? blindedPathTlv = null)
        : base(MessageTypes.UPDATE_ADD_HTLC, payload)
    {
        BlindedPathTlv = blindedPathTlv;

        if (BlindedPathTlv is not null)
        {
            Extension = new TlvStream();
            Extension.Add(BlindedPathTlv);
        }
    }

    /// <summary>
    /// Deserialize a UpdateAddHtlcMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized UpdateAddHtlcMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing UpdateAddHtlcMessage</exception>
    public static async Task<UpdateAddHtlcMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await UpdateAddHtlcPayload.DeserializeAsync(stream);

            // Deserialize extension
            var extension = await TlvStream.DeserializeAsync(stream);
            if (extension is null)
            {
                return new UpdateAddHtlcMessage(payload);
            }

            var blindedPathTlv = extension.TryGetTlv(TlvConstants.BLINDED_PATH, out var tlv)
                ? BlindedPathTlv.FromTlv(tlv!)
                : null;

            return new UpdateAddHtlcMessage(payload, blindedPathTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing UpdateAddHtlcMessage", e);
        }
    }
}