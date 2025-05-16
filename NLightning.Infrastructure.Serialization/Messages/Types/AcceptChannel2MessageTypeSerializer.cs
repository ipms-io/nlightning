using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Messages.Types;

using Domain.Protocol.Constants;
using Domain.Protocol.Factories.Interfaces;
using Domain.Protocol.Messages;
using Domain.Protocol.Messages.Interfaces;
using Domain.Protocol.Payloads;
using Domain.Protocol.Tlv;
using Domain.Serialization.Factories;
using Domain.Serialization.Messages;
using Domain.Serialization.Tlv;
using Exceptions;
using Interfaces;

public class AcceptChannel2MessageTypeSerializer : IMessageTypeSerializer<AcceptChannel2Message>
{
    private readonly IPayloadTypeSerializerFactory _payloadTypeSerializerFactory;
    private readonly ITlvConverterFactory _tlvConverterFactory;
    private readonly ITlvSerializer _tlvSerializer;
    private readonly ITlvStreamSerializer _tlvStreamSerializer;

    public AcceptChannel2MessageTypeSerializer(IPayloadTypeSerializerFactory payloadTypeSerializerFactory,
                                               ITlvConverterFactory tlvConverterFactory, ITlvSerializer tlvSerializer,
                                               ITlvStreamSerializer tlvStreamSerializer)
    {
        _payloadTypeSerializerFactory = payloadTypeSerializerFactory;
        _tlvConverterFactory = tlvConverterFactory;
        _tlvSerializer = tlvSerializer;
        _tlvStreamSerializer = tlvStreamSerializer;
    }
    
    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not AcceptChannel2Message acceptChannel2Message)
            throw new SerializationException($"Message is not of type {nameof(AcceptChannel2Message)}");
        
        // Get the payload serializer
        var payloadTypeSerializer = _payloadTypeSerializerFactory.GetSerializer(message.Type) 
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);

        if (acceptChannel2Message.UpfrontShutdownScriptTlv is not null)
        {
            var tlvConverter =
                _tlvConverterFactory.GetConverter<UpfrontShutdownScriptTlv>()
                ?? throw new SerializationException(
                    $"No converter found for tlv type {nameof(UpfrontShutdownScriptTlv)}");
            var baseTlv = tlvConverter.ConvertToBase(acceptChannel2Message.UpfrontShutdownScriptTlv);
            await _tlvSerializer.SerializeAsync(baseTlv, stream);
        }

        if (acceptChannel2Message.ChannelTypeTlv is not null)
        {
            var tlvConverter =
                _tlvConverterFactory.GetConverter<ChannelTypeTlv>()
                ?? throw new SerializationException(
                    $"No converter found for tlv type {nameof(ChannelTypeTlv)}");
            var baseTlv = tlvConverter.ConvertToBase(acceptChannel2Message.ChannelTypeTlv);
            await _tlvSerializer.SerializeAsync(baseTlv, stream);
        }
        
        if (acceptChannel2Message.RequireConfirmedInputsTlv is not null)
        {
            var tlvConverter =
                _tlvConverterFactory.GetConverter<RequireConfirmedInputsTlv>()
                ?? throw new SerializationException(
                    $"No converter found for tlv type {nameof(RequireConfirmedInputsTlv)}");
            var baseTlv = tlvConverter.ConvertToBase(acceptChannel2Message.RequireConfirmedInputsTlv);
            await _tlvSerializer.SerializeAsync(baseTlv, stream);
        }
    }

    /// <summary>
    /// Deserialize an OpenChannel2Message from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized AcceptChannel2Message.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing OpenChannel2Message</exception>
    public async Task<AcceptChannel2Message> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payloadSerializer = _payloadTypeSerializerFactory.GetSerializer<AcceptChannel2Payload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");
            
            // Deserialize extension
            if (stream.Position >= stream.Length)
                return new AcceptChannel2Message(payload);

            var extension = await _tlvStreamSerializer.DeserializeAsync(stream);
            if (extension is null)
                return new AcceptChannel2Message(payload);

            UpfrontShutdownScriptTlv? upfrontShutdownScriptTlv = null;
            if (extension.TryGetTlv(TlvConstants.UPFRONT_SHUTDOWN_SCRIPT, out var baseUpfrontTlv))
            {
                var tlvConverter = _tlvConverterFactory.GetConverter<UpfrontShutdownScriptTlv>()
                                   ?? throw new SerializationException(
                                       $"No serializer found for tlv type {nameof(UpfrontShutdownScriptTlv)}");
                upfrontShutdownScriptTlv = tlvConverter.ConvertFromBase(baseUpfrontTlv!);
            }

            ChannelTypeTlv? channelTypeTlv = null;
            if (extension.TryGetTlv(TlvConstants.UPFRONT_SHUTDOWN_SCRIPT, out var baseChannelTypeTlv))
            {
                var tlvConverter =
                    _tlvConverterFactory.GetConverter<ChannelTypeTlv>()
                    ?? throw new SerializationException($"No serializer found for tlv type {nameof(ChannelTypeTlv)}");
                channelTypeTlv = tlvConverter.ConvertFromBase(baseChannelTypeTlv!);
            }

            RequireConfirmedInputsTlv? requireConfirmedInputsTlv = null;
            if (extension.TryGetTlv(TlvConstants.UPFRONT_SHUTDOWN_SCRIPT, out var baseRequireTlv))
            {
                var tlvConverter =
                    _tlvConverterFactory.GetConverter<RequireConfirmedInputsTlv>()
                    ?? throw new SerializationException(
                        $"No serializer found for tlv type {nameof(RequireConfirmedInputsTlv)}");
                requireConfirmedInputsTlv = tlvConverter.ConvertFromBase(baseRequireTlv!);
            }

            return new AcceptChannel2Message(payload, upfrontShutdownScriptTlv, channelTypeTlv,
                    requireConfirmedInputsTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing AcceptChannel2Message", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}