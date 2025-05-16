using System.Runtime.Serialization;
using NLightning.Domain.Protocol.Tlv;

namespace NLightning.Infrastructure.Serialization.Messages.Types;

using Domain.Protocol.Messages;
using Domain.Protocol.Messages.Interfaces;
using Domain.Protocol.Payloads;
using Domain.Serialization.Factories;
using Domain.Serialization.Messages;
using Exceptions;

public class ClosingSignedMessageTypeSerializer : IMessageTypeSerializer<ClosingSignedMessage>
{
    private readonly IPayloadTypeSerializerFactory _payloadTypeSerializerFactory;
    private readonly ITlvTypeSerializerFactory _tlvTypeSerializerFactory;

    public ClosingSignedMessageTypeSerializer(IPayloadTypeSerializerFactory payloadTypeSerializerFactory,
                                          ITlvTypeSerializerFactory tlvTypeSerializerFactory)
    {
        _payloadTypeSerializerFactory = payloadTypeSerializerFactory;
        _tlvTypeSerializerFactory = tlvTypeSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not ClosingSignedMessage closingSignedMessage)
            throw new SerializationException("Message is not of type ClosingSignedMessage");
            
        // Get the payload serializer
        var payloadTypeSerializer = _payloadTypeSerializerFactory.GetSerializer(message.Type) 
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);
        
        var tlvSerializer =
            _tlvTypeSerializerFactory.GetSerializer<FeeRangeTlv>()
            ?? throw new SerializationException($"No serializer found for tlv type {nameof(FeeRangeTlv)}");
        await tlvSerializer.SerializeAsync(closingSignedMessage.FeeRangeTlv, stream);
    }

    /// <summary>
    /// Deserialize a ClosingSignedMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized AcceptChannel2Message.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing ClosingSignedMessage</exception>
    public async Task<ClosingSignedMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payloadSerializer = _payloadTypeSerializerFactory.GetSerializer<ClosingSignedPayload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");

            // Deserialize extension
            if (stream.Position >= stream.Length)
                throw new SerializationException("Required extension is missing");
            
            var feeRangeTlvSerializer =
                _tlvTypeSerializerFactory.GetSerializer<FeeRangeTlv>()
                ?? throw new SerializationException(
                    $"No serializer found for tlv type {nameof(FeeRangeTlv)}");
            var feeRangeTlv = await feeRangeTlvSerializer.DeserializeAsync(stream)
                              ?? throw new SerializationException($"Error serializing {nameof(FeeRangeTlv)}");

            return new ClosingSignedMessage(payload, feeRangeTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing ClosingSignedMessage", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}