using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Messages.Types;

using Domain.Protocol.Messages;
using Domain.Protocol.Messages.Interfaces;
using Domain.Protocol.Payloads;
using Domain.Serialization.Factories;
using Domain.Serialization.Messages.Types;
using Exceptions;

public class WarningMessageTypeSerializer : IMessageTypeSerializer<WarningMessage>
{
    private readonly IPayloadSerializerFactory _payloadSerializerFactory;

    public WarningMessageTypeSerializer(IPayloadSerializerFactory payloadSerializerFactory)
    {
        _payloadSerializerFactory = payloadSerializerFactory;
    }

    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not WarningMessage)
            throw new SerializationException("Message is not of type WarningMessage");

        // Get the payload serializer
        var payloadTypeSerializer = _payloadSerializerFactory.GetSerializer(message.Type)
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);
    }

    /// <summary>
    /// Deserialize a WarningMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized WarningMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing WarningMessage</exception>
    public async Task<WarningMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payloadSerializer = _payloadSerializerFactory.GetSerializer<ErrorPayload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");

            return new WarningMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing WarningMessage", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}