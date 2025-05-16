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

public class TxInitRbfMessageTypeSerializer : IMessageTypeSerializer<TxInitRbfMessage>
{
    private readonly IPayloadTypeSerializerFactory _payloadTypeSerializerFactory;
    private readonly ITlvConverterFactory _tlvConverterFactory;
    private readonly ITlvStreamSerializer _tlvStreamSerializer;

    public TxInitRbfMessageTypeSerializer(IPayloadTypeSerializerFactory payloadTypeSerializerFactory,
                                          ITlvConverterFactory tlvConverterFactory,
                                          ITlvStreamSerializer tlvStreamSerializer)
    {
        _payloadTypeSerializerFactory = payloadTypeSerializerFactory;
        _tlvConverterFactory = tlvConverterFactory;
        _tlvStreamSerializer = tlvStreamSerializer;
    }
    
    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not TxInitRbfMessage txAckRbfMessage)
            throw new SerializationException("Message is not of type TxInitRbfMessage");
            
        // Get the payload serializer
        var payloadTypeSerializer = _payloadTypeSerializerFactory.GetSerializer(message.Type) 
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);

        // Serialize the TLV stream
        await _tlvStreamSerializer.SerializeAsync(txAckRbfMessage.Extension, stream);
    }

    /// <summary>
    /// Deserialize a TxInitRbfMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxInitRbfMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing TxInitRbfMessage</exception>
    public async Task<TxInitRbfMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payloadSerializer = _payloadTypeSerializerFactory.GetSerializer<TxInitRbfPayload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");

            // Deserialize extension
            if (stream.Position >= stream.Length)
                return new TxInitRbfMessage(payload);

            var extension = await _tlvStreamSerializer.DeserializeAsync(stream);
            if (extension is null)
                return new TxInitRbfMessage(payload);

            FundingOutputContributionTlv? fundingOutputContributionTlv = null;
            if (extension.TryGetTlv(TlvConstants.FUNDING_OUTPUT_CONTRIBUTION, out var baseFundingOutputContributionTlv))
            {
                var tlvConverter = _tlvConverterFactory.GetConverter<FundingOutputContributionTlv>()
                                   ?? throw new SerializationException(
                                       $"No serializer found for tlv type {nameof(FundingOutputContributionTlv)}");
                fundingOutputContributionTlv = tlvConverter.ConvertFromBase(baseFundingOutputContributionTlv!);
            }

            RequireConfirmedInputsTlv? requireConfirmedInputsTlv = null;
            if (extension.TryGetTlv(TlvConstants.REQUIRE_CONFIRMED_INPUTS, out var baserequireConfirmedInputsTlv))
            {
                var tlvConverter =
                    _tlvConverterFactory.GetConverter<RequireConfirmedInputsTlv>()
                    ?? throw new SerializationException(
                        $"No serializer found for tlv type {nameof(RequireConfirmedInputsTlv)}");
                requireConfirmedInputsTlv = tlvConverter.ConvertFromBase(baserequireConfirmedInputsTlv!);
            }

            return new TxInitRbfMessage(payload, fundingOutputContributionTlv, requireConfirmedInputsTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing TxInitRbfMessage", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}