using System.Runtime.Serialization;
using NLightning.Domain.Protocol.Tlv;

namespace NLightning.Infrastructure.Serialization.Messages.Types;

using Domain.Protocol.Messages;
using Domain.Protocol.Messages.Interfaces;
using Domain.Protocol.Payloads;
using Domain.Serialization.Factories;
using Domain.Serialization.Messages;
using Exceptions;

public class OpenChannel2MessageTypeSerializer : IMessageTypeSerializer<OpenChannel2Message>
{
    private readonly IPayloadTypeSerializerFactory _payloadTypeSerializerFactory;
    private readonly ITlvTypeSerializerFactory _tlvTypeSerializerFactory;

    public OpenChannel2MessageTypeSerializer(IPayloadTypeSerializerFactory payloadTypeSerializerFactory,
                                         ITlvTypeSerializerFactory tlvTypeSerializerFactory)
    {
        _payloadTypeSerializerFactory = payloadTypeSerializerFactory;
        _tlvTypeSerializerFactory = tlvTypeSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not OpenChannel2Message openChannel2Message)
            throw new SerializationException("Message is not of type OpenChannel2Message");
            
        // Get the payload serializer
        var payloadTypeSerializer = _payloadTypeSerializerFactory.GetSerializer(message.Type) 
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);

        if (openChannel2Message.UpfrontShutdownScriptTlv is not null)
        {
            var tlvSerializer =
                _tlvTypeSerializerFactory.GetSerializer<UpfrontShutdownScriptTlv>()
                ?? throw new SerializationException(
                    $"No serializer found for tlv type {nameof(UpfrontShutdownScriptTlv)}");
            await tlvSerializer.SerializeAsync(openChannel2Message.UpfrontShutdownScriptTlv, stream);
        }

        if (openChannel2Message.ChannelTypeTlv is not null)
        {
            var tlvSerializer = 
                _tlvTypeSerializerFactory.GetSerializer<ChannelTypeTlv>()
                ?? throw new SerializationException($"No serializer found for tlv type {nameof(ChannelTypeTlv)}");
            await tlvSerializer.SerializeAsync(openChannel2Message.ChannelTypeTlv, stream);
        }
        
        if (openChannel2Message.RequireConfirmedInputsTlv is not null)
        {
            var tlvSerializer = 
                _tlvTypeSerializerFactory.GetSerializer<RequireConfirmedInputsTlv>()
                ?? throw new SerializationException(
                    $"No serializer found for tlv type {nameof(RequireConfirmedInputsTlv)}");
            await tlvSerializer.SerializeAsync(openChannel2Message.RequireConfirmedInputsTlv, stream);
        }
    }
    
    /// <summary>
    /// Deserialize an OpenChannel2Message from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized OpenChannel2Message.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing OpenChannel2Message</exception>
    public async Task<OpenChannel2Message> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payloadSerializer = _payloadTypeSerializerFactory.GetSerializer<OpenChannel2Payload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");

            // Deserialize extension if available
            if (stream.Position >= stream.Length)
                return new OpenChannel2Message(payload);

            var upfrontShutdownScriptTlvSerializer =
                _tlvTypeSerializerFactory.GetSerializer<UpfrontShutdownScriptTlv>()
                ?? throw new SerializationException(
                    $"No serializer found for tlv type {nameof(UpfrontShutdownScriptTlv)}");
            var upfrontShutdownScriptTlv = await upfrontShutdownScriptTlvSerializer.DeserializeAsync(stream);
            
            var channelTypeTlvSerializer =
                _tlvTypeSerializerFactory.GetSerializer<ChannelTypeTlv>()
                ?? throw new SerializationException($"No serializer found for tlv type {nameof(ChannelTypeTlv)}");
            var channelTypeTlv = await channelTypeTlvSerializer.DeserializeAsync(stream);

            var requireConfirmedInputsTlvSerializer =
                _tlvTypeSerializerFactory.GetSerializer<RequireConfirmedInputsTlv>()
                ?? throw new SerializationException(
                    $"No serializer found for tlv type {nameof(RequireConfirmedInputsTlv)}");
            var requireConfirmedInputsTlv = await requireConfirmedInputsTlvSerializer.DeserializeAsync(stream);

            return new OpenChannel2Message(payload, upfrontShutdownScriptTlv, channelTypeTlv,
                                           requireConfirmedInputsTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing OpenChannel2Message", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}