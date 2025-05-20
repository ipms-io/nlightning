using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Messages.Types;

using Domain.Protocol.Messages;
using Domain.Protocol.Messages.Interfaces;
using Domain.Protocol.Payloads;
using Domain.Serialization.Factories;
using Domain.Serialization.Messages.Types;
using Exceptions;

public class UpdateFailHtlcMessageTypeSerializer : IMessageTypeSerializer<UpdateFailHtlcMessage>
{
    private readonly IPayloadSerializerFactory _payloadSerializerFactory;

    public UpdateFailHtlcMessageTypeSerializer(IPayloadSerializerFactory payloadSerializerFactory)
    {
        _payloadSerializerFactory = payloadSerializerFactory;
    }

    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not UpdateFailHtlcMessage)
            throw new SerializationException("Message is not of type UpdateFailHtlcMessage");

        // Get the payload serializer
        var payloadTypeSerializer = _payloadSerializerFactory.GetSerializer(message.Type)
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);
    }

    /// <summary>
    /// Deserialize an UpdateFailHtlcMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized UpdateFailHtlcMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing UpdateFailHtlcMessage</exception>
    public async Task<UpdateFailHtlcMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payloadSerializer = _payloadSerializerFactory.GetSerializer<UpdateFailHtlcPayload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");

            return new UpdateFailHtlcMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing UpdateFailHtlcMessage", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}