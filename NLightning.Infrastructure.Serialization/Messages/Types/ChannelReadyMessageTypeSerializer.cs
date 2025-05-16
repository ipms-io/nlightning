using System.Runtime.Serialization;
using NLightning.Domain.Protocol.Messages;
using NLightning.Domain.Protocol.Messages.Interfaces;
using NLightning.Domain.Protocol.Payloads;
using NLightning.Domain.Protocol.Tlv;
using NLightning.Domain.Serialization.Factories;
using NLightning.Domain.Serialization.Messages;
using NLightning.Infrastructure.Exceptions;

namespace NLightning.Infrastructure.Serialization.Messages.Types;

public class ChannelReadyMessageTypeSerializer : IMessageTypeSerializer<ChannelReadyMessage>
{
    private readonly IPayloadTypeSerializerFactory _payloadTypeSerializerFactory;
    private readonly ITlvTypeSerializerFactory _tlvTypeSerializerFactory;

    public ChannelReadyMessageTypeSerializer(IPayloadTypeSerializerFactory payloadTypeSerializerFactory,
                                         ITlvTypeSerializerFactory tlvTypeSerializerFactory)
    {
        _payloadTypeSerializerFactory = payloadTypeSerializerFactory;
        _tlvTypeSerializerFactory = tlvTypeSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not ChannelReadyMessage channelReadyMessage)
            throw new SerializationException($"Message is not of type {nameof(ChannelReadyMessage)}");
        
        // Get the payload serializer
        var payloadTypeSerializer = _payloadTypeSerializerFactory.GetSerializer(message.Type) 
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);

        if (channelReadyMessage.ShortChannelIdTlv is not null)
        {
            var tlvSerializer =
                _tlvTypeSerializerFactory.GetSerializer<ShortChannelIdTlv>()
                ?? throw new SerializationException(
                    $"No serializer found for tlv type {nameof(ShortChannelIdTlv)}");
            await tlvSerializer.SerializeAsync(channelReadyMessage.ShortChannelIdTlv, stream);
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
            var payloadSerializer = _payloadTypeSerializerFactory.GetSerializer<ChannelReadyPayload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");

            // Deserialize extension
            if (stream.Position >= stream.Length)
                return new ChannelReadyMessage(payload);

            var shortChannelIdTlvSerializer =
                _tlvTypeSerializerFactory.GetSerializer<ShortChannelIdTlv>()
                ?? throw new SerializationException(
                    $"No serializer found for tlv type {nameof(ShortChannelIdTlv)}");
            var shortChannelIdTlv = await shortChannelIdTlvSerializer.DeserializeAsync(stream);

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