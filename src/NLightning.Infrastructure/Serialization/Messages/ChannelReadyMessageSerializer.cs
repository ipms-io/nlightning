using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Messages;

using Application.Interfaces.Serialization;
using Common.BitUtils;
using Domain.Protocol.Constants;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Messages;
using Domain.Protocol.Payloads;
using Domain.Protocol.Tlvs;
using Domain.ValueObjects;
using Exceptions;

public class ChannelReadyMessageSerializer : IMessageTypeSerializer<ChannelReadyMessage>
{
    private readonly IPayloadSerializer _payloadSerializer;
    
    public ChannelReadyMessageSerializer(IPayloadSerializer payloadSerializer)
    {
        _payloadSerializer = payloadSerializer;
    }
    
    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(message.Type));
        await _payloadSerializer.SerializeAsync(message.Payload, stream);
        
        if (message.Extension?.Any() ?? false)
        {
            foreach (var tlv in message.Extension.GetTlvs())
            {
                await tlv.SerializeAsync(stream);
            }
        }
    }

    /// <summary>
    /// Deserialize a ChannelReadyMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized ChannelReadyMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing ChannelReadyMessage</exception>
    public async Task<ChannelReadyMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await _payloadSerializer.DeserializeAsync<ChannelReadyPayload>(stream);

            // Deserialize extension
            var extension = await TlvStream.DeserializeAsync(stream);
            if (extension is null)
            {
                return new ChannelReadyMessage(payload);
            }

            var shortChannelIdTlv = extension.TryGetTlv(TlvConstants.SHORT_CHANNEL_ID, out var tlv)
                ? ShortChannelIdTlv.FromTlv(tlv!)
                : null;

            return new ChannelReadyMessage(payload, shortChannelIdTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing ChannelReadyMessage", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}