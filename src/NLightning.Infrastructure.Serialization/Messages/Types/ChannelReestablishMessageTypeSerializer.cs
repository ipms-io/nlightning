using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Messages.Types;

using Domain.Protocol.Constants;
using Domain.Protocol.Factories;
using Domain.Protocol.Messages;
using Domain.Protocol.Messages.Interfaces;
using Domain.Protocol.Payloads;
using Domain.Protocol.Tlv;
using Domain.Serialization.Factories;
using Domain.Serialization.Messages.Types;
using Exceptions;
using Interfaces;

public class ChannelReestablishMessageTypeSerializer : IMessageTypeSerializer<ChannelReestablishMessage>
{
    private readonly IPayloadSerializerFactory _payloadSerializerFactory;
    private readonly ITlvConverterFactory _tlvConverterFactory;
    private readonly ITlvStreamSerializer _tlvStreamSerializer;

    public ChannelReestablishMessageTypeSerializer(IPayloadSerializerFactory payloadSerializerFactory,
                                                   ITlvConverterFactory tlvConverterFactory,
                                                   ITlvStreamSerializer tlvStreamSerializer)
    {
        _payloadSerializerFactory = payloadSerializerFactory;
        _tlvConverterFactory = tlvConverterFactory;
        _tlvStreamSerializer = tlvStreamSerializer;
    }

    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not ChannelReestablishMessage channelReestablishMessage)
            throw new SerializationException($"Message is not of type {nameof(ChannelReestablishMessage)}");

        // Get the payload serializer
        var payloadTypeSerializer = _payloadSerializerFactory.GetSerializer(message.Type)
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);

        // Serialize the TLV stream
        await _tlvStreamSerializer.SerializeAsync(channelReestablishMessage.Extension, stream);
    }

    /// <summary>
    /// Deserialize a ChannelReestablishMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized ChannelReestablishMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing ChannelReestablishMessage</exception>
    public async Task<ChannelReestablishMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payloadSerializer = _payloadSerializerFactory.GetSerializer<ChannelReestablishPayload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");

            // Deserialize extension
            if (stream.Position >= stream.Length)
                return new ChannelReestablishMessage(payload);

            var extension = await _tlvStreamSerializer.DeserializeAsync(stream);
            if (extension is null)
                return new ChannelReestablishMessage(payload);

            NextFundingTlv? nextFundingTlv = null;
            if (extension.TryGetTlv(TlvConstants.NEXT_FUNDING, out var baseNextFundingTlv))
            {
                var tlvConverter = _tlvConverterFactory.GetConverter<NextFundingTlv>()
                                   ?? throw new SerializationException(
                                       $"No serializer found for tlv type {nameof(NextFundingTlv)}");
                nextFundingTlv = tlvConverter.ConvertFromBase(baseNextFundingTlv!);
            }

            return new ChannelReestablishMessage(payload, nextFundingTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing ChannelReestablishMessage", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}