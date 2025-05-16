using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Messages.Types;

using Domain.Protocol.Messages;
using Domain.Protocol.Messages.Interfaces;
using Domain.Protocol.Payloads;
using Domain.Serialization.Factories;
using Domain.Serialization.Messages;
using Exceptions;

public class UpdateAddHtlcMessageTypeMessageTypeSerializer : IMessageTypeSerializer<UpdateAddHtlcMessage>
{
    private readonly IPayloadTypeSerializerFactory _payloadTypeSerializerFactory;
    private readonly ITlvTypeSerializerFactory _tlvTypeSerializerFactory;

    public UpdateAddHtlcMessageTypeMessageTypeSerializer(IPayloadTypeSerializerFactory payloadTypeSerializerFactory,
                                         ITlvTypeSerializerFactory tlvTypeSerializerFactory)
    {
        _payloadTypeSerializerFactory = payloadTypeSerializerFactory;
        _tlvTypeSerializerFactory = tlvTypeSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not UpdateAddHtlcMessage updateAddHtlcMessage)
            throw new SerializationException("Message is not of type UpdateAddHtlcMessage");
            
        // Get the payload serializer
        var payloadTypeSerializer = _payloadTypeSerializerFactory.GetSerializer(message.Type) 
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);
        
        if (updateAddHtlcMessage.BlindedPathTlv is not null)
        {
            var tlvSerializer =
                _tlvTypeSerializerFactory.GetSerializer<BlindedPathTlv>()
                ?? throw new SerializationException(
                    $"No serializer found for tlv type {nameof(BlindedPathTlv)}");
            await tlvSerializer.SerializeAsync(updateAddHtlcMessage.BlindedPathTlv, stream);
        }
    }
    
    /// <summary>
    /// Deserialize an UpdateAddHtlcMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized UpdateAddHtlcMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing UpdateAddHtlcMessage</exception>
    public async Task<UpdateAddHtlcMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payloadSerializer = _payloadTypeSerializerFactory.GetSerializer<UpdateAddHtlcPayload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");

            // Deserialize extension if available
            if (stream.Position >= stream.Length)
                return new UpdateAddHtlcMessage(payload);

            var blindedPathTlvSerializer =
                _tlvTypeSerializerFactory.GetSerializer<BlindedPathTlv>()
                ?? throw new SerializationException(
                    $"No serializer found for tlv type {nameof(BlindedPathTlv)}");
            var blindedPathTlv = await blindedPathTlvSerializer.DeserializeAsync(stream);

            return new UpdateAddHtlcMessage(payload, blindedPathTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing UpdateAddHtlcMessage", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}