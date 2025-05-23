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

public class OpenChannel2MessageTypeSerializer : IMessageTypeSerializer<OpenChannel2Message>
{
    private readonly IPayloadSerializerFactory _payloadSerializerFactory;
    private readonly ITlvConverterFactory _tlvConverterFactory;
    private readonly ITlvStreamSerializer _tlvStreamSerializer;

    public OpenChannel2MessageTypeSerializer(IPayloadSerializerFactory payloadSerializerFactory,
                                             ITlvConverterFactory tlvConverterFactory,
                                             ITlvStreamSerializer tlvStreamSerializer)
    {
        _payloadSerializerFactory = payloadSerializerFactory;
        _tlvConverterFactory = tlvConverterFactory;
        _tlvStreamSerializer = tlvStreamSerializer;
    }

    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not OpenChannel2Message openChannel2Message)
            throw new SerializationException("Message is not of type OpenChannel2Message");

        // Get the payload serializer
        var payloadTypeSerializer = _payloadSerializerFactory.GetSerializer(message.Type)
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);

        // Serialize the TLV stream
        await _tlvStreamSerializer.SerializeAsync(openChannel2Message.Extension, stream);
    }

    /// <summary>
    /// Deserialize an OpenChannel2Message from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized OpenChannel2Message.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing OpenChannel2Message</exception>
    public async Task<OpenChannel2Message> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payloadSerializer = _payloadSerializerFactory.GetSerializer<OpenChannel2Payload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");

            // Deserialize extension if available
            if (stream.Position >= stream.Length)
                return new OpenChannel2Message(payload);

            var extension = await _tlvStreamSerializer.DeserializeAsync(stream);
            if (extension is null)
                return new OpenChannel2Message(payload);

            UpfrontShutdownScriptTlv? upfrontShutdownScriptTlv = null;
            if (extension.TryGetTlv(TlvConstants.UpfrontShutdownScript, out var baseUpfrontShutdownTlv))
            {
                var tlvConverter = _tlvConverterFactory.GetConverter<UpfrontShutdownScriptTlv>()
                                   ?? throw new SerializationException(
                                       $"No serializer found for tlv type {nameof(UpfrontShutdownScriptTlv)}");
                upfrontShutdownScriptTlv = tlvConverter.ConvertFromBase(baseUpfrontShutdownTlv!);
            }

            ChannelTypeTlv? channelTypeTlv = null;
            if (extension.TryGetTlv(TlvConstants.ChannelType, out var baseChannelTypeTlv))
            {
                var tlvConverter =
                    _tlvConverterFactory.GetConverter<ChannelTypeTlv>()
                    ?? throw new SerializationException($"No serializer found for tlv type {nameof(ChannelTypeTlv)}");
                channelTypeTlv = tlvConverter.ConvertFromBase(baseChannelTypeTlv!);
            }

            RequireConfirmedInputsTlv? requireConfirmedInputsTlv = null;
            if (extension.TryGetTlv(TlvConstants.RequireConfirmedInputs, out var baseRequireConfirmedInputsTlv))
            {
                var tlvConverter =
                    _tlvConverterFactory.GetConverter<RequireConfirmedInputsTlv>()
                    ?? throw new SerializationException(
                        $"No serializer found for tlv type {nameof(RequireConfirmedInputsTlv)}");
                requireConfirmedInputsTlv = tlvConverter.ConvertFromBase(baseRequireConfirmedInputsTlv!);
            }

            return new OpenChannel2Message(payload, upfrontShutdownScriptTlv, channelTypeTlv,
                                           requireConfirmedInputsTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing OpenChannel2Message", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}