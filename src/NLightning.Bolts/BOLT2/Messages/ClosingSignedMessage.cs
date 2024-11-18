using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Bolts.Constants;
using Common.Constants;
using Common.TLVs;
using Exceptions;
using Payloads;

/// <summary>
/// Represents a closing_signed message.
/// </summary>
/// <remarks>
/// The closing_signed message is after shutdown is complete and there are no pending HTLCs.
/// The message type is 39.
/// </remarks>
/// <param name="payload"></param>
public sealed class ClosingSignedMessage : BaseMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new ClosingSignedPayload Payload { get => (ClosingSignedPayload)base.Payload; }

    public FeeRangeTlv FeeRange { get; set; }

    public ClosingSignedMessage(ClosingSignedPayload payload, FeeRangeTlv feeRange) : base(MessageTypes.CLOSING_SIGNED, payload)
    {
        FeeRange = feeRange;
        Extension = new TlvStream();
        Extension.Add(feeRange);
    }

    /// <summary>
    /// Deserialize a OpenChannel2Message from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized AcceptChannel2Message.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing OpenChannel2Message</exception>
    public static async Task<ClosingSignedMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await ClosingSignedPayload.DeserializeAsync(stream);

            // Deserialize extension
            var extension = await TlvStream.DeserializeAsync(stream) ?? throw new SerializationException("Required extension is missing");
            if (!extension.TryGetTlv(TlvConstants.FEE_RANGE, out var tlv))
            {
                throw new SerializationException("Required extension is missing");
            }

            return new ClosingSignedMessage(payload, new FeeRangeTlv(tlv ?? throw new SerializationException("Required extension is missing")));
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing ClosingSignedMessage", e);
        }
    }
}