using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Common.Constants;
using Common.TLVs;
using Payloads;

/// <summary>
/// Represents a closing_signed message.
/// </summary>
/// <remarks>
/// The closing_signed message is after shutdown is complete and there are no pending HTLCs.
/// The message type is 39.
/// </remarks>
public sealed class ClosingSignedMessage : BaseMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new ClosingSignedPayload Payload { get => (ClosingSignedPayload)base.Payload; }

    public FeeRangeTlv FeeRangeTlv { get; }

    public ClosingSignedMessage(ClosingSignedPayload payload, FeeRangeTlv feeRangeTlv) : base(MessageTypes.CLOSING_SIGNED, payload)
    {
        FeeRangeTlv = feeRangeTlv;
        Extension = new TlvStream();
        Extension.Add(feeRangeTlv);
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
            if (!extension.TryGetTlv(TlvConstants.FEE_RANGE, out var feeRangeTlv))
            {
                throw new SerializationException("Required extension is missing");
            }

            return new ClosingSignedMessage(payload, FeeRangeTlv.FromTlv(feeRangeTlv!));
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing ClosingSignedMessage", e);
        }
    }
}