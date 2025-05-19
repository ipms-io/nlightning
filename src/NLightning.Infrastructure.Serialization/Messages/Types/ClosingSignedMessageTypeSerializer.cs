using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Messages.Types;

using Domain.Protocol.Constants;
using Domain.Protocol.Factories;
using Domain.Protocol.Messages;
using Domain.Protocol.Messages.Interfaces;
using Domain.Protocol.Payloads;
using Domain.Protocol.Tlv;
using Domain.Serialization.Factories;
using Domain.Serialization.Messages.Types;
using Exceptions;
using Interfaces;

public class ClosingSignedMessageTypeSerializer : IMessageTypeSerializer<ClosingSignedMessage>
{
    private readonly IPayloadSerializerFactory _payloadSerializerFactory;
    private readonly ITlvConverterFactory _tlvConverterFactory;
    private readonly ITlvStreamSerializer _tlvStreamSerializer;

    public ClosingSignedMessageTypeSerializer(IPayloadSerializerFactory payloadSerializerFactory,
                                              ITlvConverterFactory tlvConverterFactory,
                                              ITlvStreamSerializer tlvStreamSerializer)
    {
        _payloadSerializerFactory = payloadSerializerFactory;
        _tlvConverterFactory = tlvConverterFactory;
        _tlvStreamSerializer = tlvStreamSerializer;
    }
    
    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not ClosingSignedMessage closingSignedMessage)
            throw new SerializationException("Message is not of type ClosingSignedMessage");
            
        // Get the payload serializer
        var payloadTypeSerializer = _payloadSerializerFactory.GetSerializer(message.Type) 
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);

        // Serialize the TLV stream
        await _tlvStreamSerializer.SerializeAsync(closingSignedMessage.Extension, stream);
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
            var payloadSerializer = _payloadSerializerFactory.GetSerializer<ClosingSignedPayload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");

            // Deserialize extension
            if (stream.Position >= stream.Length)
                throw new SerializationException("Required extension is missing");
            
            var extension = await _tlvStreamSerializer.DeserializeAsync(stream);
            if (extension is null || !extension.TryGetTlv(TlvConstants.FEE_RANGE, out var baseFeeRangeTlv))
                throw new SerializationException("Required extension is missing");

            var tlvConverter = _tlvConverterFactory.GetConverter<FeeRangeTlv>()
                               ?? throw new SerializationException(
                                   $"No serializer found for tlv type {nameof(FeeRangeTlv)}");
            var feeRangeTlv = tlvConverter.ConvertFromBase(baseFeeRangeTlv!);

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