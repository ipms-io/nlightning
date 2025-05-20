using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Messages.Types;

using Domain.Protocol.Messages;
using Domain.Protocol.Messages.Interfaces;
using Domain.Protocol.Payloads;
using Domain.Serialization.Factories;
using Domain.Serialization.Messages.Types;
using Exceptions;

public class RevokeAndAckMessageTypeSerializer : IMessageTypeSerializer<RevokeAndAckMessage>
{
    private readonly IPayloadSerializerFactory _payloadSerializerFactory;

    public RevokeAndAckMessageTypeSerializer(IPayloadSerializerFactory payloadSerializerFactory)
    {
        _payloadSerializerFactory = payloadSerializerFactory;
    }

    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not RevokeAndAckMessage)
            throw new SerializationException("Message is not of type RevokeAndAckMessage");

        // Get the payload serializer
        var payloadTypeSerializer = _payloadSerializerFactory.GetSerializer(message.Type)
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);
    }

    /// <summary>
    /// Deserialize a RevokeAndAckMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized RevokeAndAckMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing RevokeAndAckMessage</exception>
    public async Task<RevokeAndAckMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payloadSerializer = _payloadSerializerFactory.GetSerializer<RevokeAndAckPayload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");

            return new RevokeAndAckMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing RevokeAndAckMessage", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}