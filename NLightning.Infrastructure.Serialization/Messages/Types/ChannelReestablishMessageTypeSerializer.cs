using System.Runtime.Serialization;
using NLightning.Domain.Protocol.Tlv;

namespace NLightning.Infrastructure.Serialization.Messages.Types;

using NLightning.Domain.Protocol.Messages;
using NLightning.Domain.Protocol.Messages.Interfaces;
using NLightning.Domain.Protocol.Payloads;
using NLightning.Domain.Serialization.Factories;
using NLightning.Domain.Serialization.Messages;
using Exceptions;

public class ChannelReestablishMessageTypeSerializer : IMessageTypeSerializer<ChannelReestablishMessage>
{
    private readonly IPayloadTypeSerializerFactory _payloadTypeSerializerFactory;
    private readonly ITlvTypeSerializerFactory _tlvTypeSerializerFactory;

    public ChannelReestablishMessageTypeSerializer(IPayloadTypeSerializerFactory payloadTypeSerializerFactory,
                                               ITlvTypeSerializerFactory tlvTypeSerializerFactory)
    {
        _payloadTypeSerializerFactory = payloadTypeSerializerFactory;
        _tlvTypeSerializerFactory = tlvTypeSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not ChannelReestablishMessage channelReestablishMessage)
            throw new SerializationException($"Message is not of type {nameof(ChannelReestablishMessage)}");
            
        // Get the payload serializer
        var payloadTypeSerializer = _payloadTypeSerializerFactory.GetSerializer(message.Type) 
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);
        
        if (channelReestablishMessage.NextFundingTlv is not null)
        {
            var tlvSerializer =
                _tlvTypeSerializerFactory.GetSerializer<NextFundingTlv>()
                ?? throw new SerializationException($"No serializer found for tlv type {nameof(NextFundingTlv)}");
            await tlvSerializer.SerializeAsync(channelReestablishMessage.NextFundingTlv, stream);
        }
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
            var payloadSerializer = _payloadTypeSerializerFactory.GetSerializer<ChannelReestablishPayload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");

            // Deserialize extension
            if (stream.Position >= stream.Length)
                return new ChannelReestablishMessage(payload);

            var nextFundingTlvSerializer =
                _tlvTypeSerializerFactory.GetSerializer<NextFundingTlv>()
                ?? throw new SerializationException(
                    $"No serializer found for tlv type {nameof(NextFundingTlv)}");
            var nextFundingTlv = await nextFundingTlvSerializer.DeserializeAsync(stream);

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