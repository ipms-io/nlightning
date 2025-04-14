using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Common.Constants;
using Common.TLVs;
using Payloads;

/// <summary>
/// Represents a channel_reestablish message.
/// </summary>
/// <remarks>
/// The channel_reestablish message is sent when a connection is lost.
/// The message type is 136.
/// </remarks>
public sealed class ChannelReestablishMessage : BaseMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new ChannelReestablishPayload Payload { get => (ChannelReestablishPayload)base.Payload; }

    public NextFundingTlv? NextFundingTlv { get; }

    public ChannelReestablishMessage(ChannelReestablishPayload payload, NextFundingTlv? nextFundingTlv = null)
        : base(MessageTypes.CHANNEL_REESTABLISH, payload)
    {
        NextFundingTlv = nextFundingTlv;

        if (NextFundingTlv is not null)
        {
            Extension = new TlvStream();
            Extension.Add(NextFundingTlv);
        }
    }

    /// <summary>
    /// Deserialize a ChannelReestablishMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized ChannelReestablishMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing ChannelReestablishMessage</exception>
    public static async Task<ChannelReestablishMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await ChannelReestablishPayload.DeserializeAsync(stream);

            // Deserialize extension
            var extension = await TlvStream.DeserializeAsync(stream);
            if (extension is null)
            {
                return new ChannelReestablishMessage(payload);
            }

            var nextFundingTlv = extension.TryGetTlv(TlvConstants.NEXT_FUNDING, out var tlv)
                ? NextFundingTlv.FromTlv(tlv!)
                : null;

            return new ChannelReestablishMessage(payload, nextFundingTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing ChannelReestablishMessage", e);
        }
    }
}