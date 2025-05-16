using System.Runtime.Serialization;
using NLightning.Domain.Protocol.Tlv;

namespace NLightning.Infrastructure.Serialization.Messages.Types;

using Domain.Protocol.Messages;
using Domain.Protocol.Messages.Interfaces;
using Domain.Protocol.Payloads;
using Domain.Serialization.Factories;
using Domain.Serialization.Messages;
using Exceptions;

public class InitMessageTypeSerializer : IMessageTypeSerializer<InitMessage>
{
    private readonly IPayloadTypeSerializerFactory _payloadTypeSerializerFactory;
    private readonly ITlvTypeSerializerFactory _tlvTypeSerializerFactory;

    public InitMessageTypeSerializer(IPayloadTypeSerializerFactory payloadTypeSerializerFactory,
                                 ITlvTypeSerializerFactory tlvTypeSerializerFactory)
    {
        _payloadTypeSerializerFactory = payloadTypeSerializerFactory;
        _tlvTypeSerializerFactory = tlvTypeSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not InitMessage initMessage)
            throw new SerializationException("Message is not of type InitMessage");
            
        // Get the payload serializer
        var payloadTypeSerializer = _payloadTypeSerializerFactory.GetSerializer(message.Type) 
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);
        
        if (initMessage.NetworksTlv is not null)
        {
            var tlvSerializer =
                _tlvTypeSerializerFactory.GetSerializer<NetworksTlv>()
                ?? throw new SerializationException(
                    $"No serializer found for tlv type {nameof(NetworksTlv)}");
            await tlvSerializer.SerializeAsync(initMessage.NetworksTlv, stream);
        }
    }

    /// <summary>
    /// Deserialize an InitMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized InitMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing InitMessage</exception>
    public async Task<InitMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payloadSerializer = _payloadTypeSerializerFactory.GetSerializer<InitPayload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");

            // Deserialize extension if available
            if (stream.Position >= stream.Length)
                return new InitMessage(payload);

            var upfrontShutdownScriptTlvSerializer =
                _tlvTypeSerializerFactory.GetSerializer<NetworksTlv>()
                ?? throw new SerializationException(
                    $"No serializer found for tlv type {nameof(NetworksTlv)}");
            var networksTlv = await upfrontShutdownScriptTlvSerializer.DeserializeAsync(stream);

            return new InitMessage(payload, networksTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing InitMessage", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}