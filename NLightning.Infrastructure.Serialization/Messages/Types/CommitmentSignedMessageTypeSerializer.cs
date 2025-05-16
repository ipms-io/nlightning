using System.Runtime.Serialization;
using NLightning.Domain.Protocol.Messages;
using NLightning.Domain.Protocol.Messages.Interfaces;
using NLightning.Domain.Protocol.Payloads;
using NLightning.Domain.Serialization.Factories;
using NLightning.Domain.Serialization.Messages;
using NLightning.Infrastructure.Exceptions;

namespace NLightning.Infrastructure.Serialization.Messages.Types;

public class CommitmentSignedMessageTypeSerializer : IMessageTypeSerializer<CommitmentSignedMessage>
{
    private readonly IPayloadTypeSerializerFactory _payloadTypeSerializerFactory;

    public CommitmentSignedMessageTypeSerializer(IPayloadTypeSerializerFactory payloadTypeSerializerFactory)
    {
        _payloadTypeSerializerFactory = payloadTypeSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not CommitmentSignedMessage)
            throw new SerializationException("Message is not of type CommitmentSignedMessage");
            
        // Get the payload serializer
        var payloadTypeSerializer = _payloadTypeSerializerFactory.GetSerializer(message.Type) 
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);
    }

    /// <summary>
    /// Deserialize a CommitmentSignedMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized CommitmentSignedMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing CommitmentSignedMessage</exception>
    public async Task<CommitmentSignedMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payloadSerializer = _payloadTypeSerializerFactory.GetSerializer<CommitmentSignedPayload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");

            return new CommitmentSignedMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing CommitmentSignedMessage", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}