using System.Runtime.Serialization;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Messages.Types;

using Domain.Protocol.Constants;
using Domain.Protocol.Messages;
using Domain.Protocol.Payloads;
using Domain.Protocol.Tlv;
using Exceptions;
using Interfaces;

public class ChannelReadyMessageTypeSerializer : IMessageTypeSerializer<ChannelReadyMessage>
{
    private readonly IPayloadSerializerFactory _payloadSerializerFactory;
    private readonly ITlvConverterFactory _tlvConverterFactory;
    private readonly ITlvStreamSerializer _tlvStreamSerializer;

    public ChannelReadyMessageTypeSerializer(IPayloadSerializerFactory payloadSerializerFactory,
                                             ITlvConverterFactory tlvConverterFactory,
                                             ITlvStreamSerializer tlvStreamSerializer)
    {
        _payloadSerializerFactory = payloadSerializerFactory;
        _tlvConverterFactory = tlvConverterFactory;
        _tlvStreamSerializer = tlvStreamSerializer;
    }

    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not ChannelReadyMessage channelReadyMessage)
            throw new SerializationException($"Message is not of type {nameof(ChannelReadyMessage)}");

        // Get the payload serializer
        var payloadTypeSerializer = _payloadSerializerFactory.GetSerializer(message.Type)
                                 ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);

        // Serialize the TLV stream
        await _tlvStreamSerializer.SerializeAsync(channelReadyMessage.Extension, stream);
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
            var payloadSerializer = _payloadSerializerFactory.GetSerializer<ChannelReadyPayload>()
                                 ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                       ?? throw new SerializationException("Error serializing payload");

            // Deserialize extension
            if (stream.Position >= stream.Length)
                return new ChannelReadyMessage(payload);

            var extension = await _tlvStreamSerializer.DeserializeAsync(stream);
            if (extension is null)
                return new ChannelReadyMessage(payload);

            ShortChannelIdTlv? shortChannelIdTlv = null;
            if (extension.TryGetTlv(TlvConstants.ShortChannelId, out var baseShortChannelId))
            {
                var tlvConverter = _tlvConverterFactory.GetConverter<ShortChannelIdTlv>()
                                ?? throw new SerializationException(
                                       $"No serializer found for tlv type {nameof(ShortChannelIdTlv)}");
                shortChannelIdTlv = tlvConverter.ConvertFromBase(baseShortChannelId!);
            }

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