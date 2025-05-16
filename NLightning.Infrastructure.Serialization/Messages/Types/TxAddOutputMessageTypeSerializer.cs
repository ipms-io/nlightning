using System.Runtime.Serialization;
using NLightning.Domain.Protocol.Messages;
using NLightning.Domain.Protocol.Messages.Interfaces;
using NLightning.Domain.Protocol.Payloads;
using NLightning.Domain.Serialization;
using NLightning.Domain.Serialization.Factories;
using NLightning.Domain.Serialization.Messages;
using NLightning.Infrastructure.Exceptions;
using NLightning.Infrastructure.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Serializers.Messages.Types;

public class TxAddOutputMessageTypeSerializer : IMessageTypeSerializer<TxAddOutputMessage>
{
    private readonly IPayloadTypeSerializerFactory _payloadTypeSerializerFactory;
    
    public TxAddOutputMessageTypeSerializer(IPayloadTypeSerializerFactory payloadTypeSerializerFactory)
    {
        _payloadTypeSerializerFactory = payloadTypeSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not TxAddOutputMessage)
            throw new SerializationException("Message is not of type TxAddOutputMessage");
            
        // Get the payload serializer
        var payloadTypeSerializer = _payloadTypeSerializerFactory.GetSerializer(message.Type) 
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);
    }

    /// <summary>
    /// Deserialize a TxAddOutputMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxAddOutputMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing TxAddOutputMessage</exception>
    public async Task<TxAddOutputMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payloadSerializer = _payloadTypeSerializerFactory.GetSerializer<TxAddOutputPayload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");

            return new TxAddOutputMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing TxAddOutputMessage", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}