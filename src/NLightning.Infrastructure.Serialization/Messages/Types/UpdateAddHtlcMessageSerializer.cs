using System.Runtime.Serialization;
using NLightning.Domain.Protocol.Factories;

namespace NLightning.Infrastructure.Serialization.Messages.Types;

using Domain.Protocol.Constants;
using Domain.Protocol.Messages;
using Domain.Protocol.Messages.Interfaces;
using Domain.Protocol.Payloads;
using Domain.Protocol.Tlv;
using Domain.Serialization.Factories;
using Domain.Serialization.Messages.Types;
using Exceptions;
using Interfaces;

public class UpdateAddHtlcMessageTypeSerializer : IMessageTypeSerializer<UpdateAddHtlcMessage>
{
    private readonly IPayloadSerializerFactory _payloadSerializerFactory;
    private readonly ITlvConverterFactory _tlvConverterFactory;
    private readonly ITlvStreamSerializer _tlvStreamSerializer;

    public UpdateAddHtlcMessageTypeSerializer(IPayloadSerializerFactory payloadSerializerFactory,
                                                         ITlvConverterFactory tlvConverterFactory,
                                                         ITlvStreamSerializer tlvStreamSerializer)
    {
        _payloadSerializerFactory = payloadSerializerFactory;
        _tlvConverterFactory = tlvConverterFactory;
        _tlvStreamSerializer = tlvStreamSerializer;
    }

    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not UpdateAddHtlcMessage updateAddHtlcMessage)
            throw new SerializationException("Message is not of type UpdateAddHtlcMessage");

        // Get the payload serializer
        var payloadTypeSerializer = _payloadSerializerFactory.GetSerializer(message.Type)
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);

        // Serialize the TLV stream
        await _tlvStreamSerializer.SerializeAsync(updateAddHtlcMessage.Extension, stream);
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
            var payloadSerializer = _payloadSerializerFactory.GetSerializer<UpdateAddHtlcPayload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");

            // Deserialize extension if available
            if (stream.Position >= stream.Length)
                return new UpdateAddHtlcMessage(payload);

            var extension = await _tlvStreamSerializer.DeserializeAsync(stream);
            if (extension is null)
                return new UpdateAddHtlcMessage(payload);

            BlindedPathTlv? blindedPathTlv = null;
            if (extension.TryGetTlv(TlvConstants.UPFRONT_SHUTDOWN_SCRIPT, out var baseBlindedPathTlv))
            {
                var tlvConverter = _tlvConverterFactory.GetConverter<BlindedPathTlv>()
                                   ?? throw new SerializationException(
                                       $"No serializer found for tlv type {nameof(BlindedPathTlv)}");
                blindedPathTlv = tlvConverter.ConvertFromBase(baseBlindedPathTlv!);
            }

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