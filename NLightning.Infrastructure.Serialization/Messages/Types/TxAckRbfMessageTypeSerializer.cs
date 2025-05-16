using System.Runtime.Serialization;
using NLightning.Domain.Protocol.Tlv;

namespace NLightning.Infrastructure.Serialization.Messages.Types;

using Domain.Protocol.Messages;
using Domain.Protocol.Messages.Interfaces;
using Domain.Protocol.Payloads;
using Domain.Serialization.Factories;
using Domain.Serialization.Messages;
using Exceptions;

public class TxAckRbfMessageTypeSerializer : IMessageTypeSerializer<TxAckRbfMessage>
{
    private readonly IPayloadTypeSerializerFactory _payloadTypeSerializerFactory;
    private readonly ITlvTypeSerializerFactory _tlvTypeSerializerFactory;

    public TxAckRbfMessageTypeSerializer(IPayloadTypeSerializerFactory payloadTypeSerializerFactory,
                                     ITlvTypeSerializerFactory tlvTypeSerializerFactory)
    {
        _payloadTypeSerializerFactory = payloadTypeSerializerFactory;
        _tlvTypeSerializerFactory = tlvTypeSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        if (message is not TxAckRbfMessage txAckRbfMessage)
            throw new SerializationException("Message is not of type TxAckRbfMessage");
            
        // Get the payload serializer
        var payloadTypeSerializer = _payloadTypeSerializerFactory.GetSerializer(message.Type) 
                                    ?? throw new SerializationException("No serializer found for payload type");
        await payloadTypeSerializer.SerializeAsync(message.Payload, stream);

        if (txAckRbfMessage.FundingOutputContributionTlv is not null)
        {
            var tlvSerializer =
                _tlvTypeSerializerFactory.GetSerializer<FundingOutputContributionTlv>()
                ?? throw new SerializationException(
                    $"No serializer found for tlv type {nameof(FundingOutputContributionTlv)}");
            await tlvSerializer.SerializeAsync(txAckRbfMessage.FundingOutputContributionTlv, stream);
        }

        if (txAckRbfMessage.RequireConfirmedInputsTlv is not null)
        {
            var tlvSerializer = 
                _tlvTypeSerializerFactory.GetSerializer<RequireConfirmedInputsTlv>()
                ?? throw new SerializationException(
                    $"No serializer found for tlv type {nameof(RequireConfirmedInputsTlv)}");
            await tlvSerializer.SerializeAsync(txAckRbfMessage.RequireConfirmedInputsTlv, stream);
        }
    }

    /// <summary>
    /// Deserialize a TxAckRbfMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxAckRbfMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing TxAckRbfMessage</exception>
    public async Task<TxAckRbfMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payloadSerializer = _payloadTypeSerializerFactory.GetSerializer<TxAckRbfPayload>()
                                    ?? throw new SerializationException("No serializer found for payload type");
            var payload = await payloadSerializer.DeserializeAsync(stream)
                          ?? throw new SerializationException("Error serializing payload");

            // Deserialize extension
            if (stream.Position >= stream.Length)
                return new TxAckRbfMessage(payload);

            var fundingOutputContributionTlvSerializer =
                _tlvTypeSerializerFactory.GetSerializer<FundingOutputContributionTlv>()
                ?? throw new SerializationException(
                    $"No serializer found for tlv type {nameof(FundingOutputContributionTlv)}");
            var fundingOutputContributionTlv = await fundingOutputContributionTlvSerializer.DeserializeAsync(stream);
            
            var requireConfirmedInputsTlvSerializer =
                _tlvTypeSerializerFactory.GetSerializer<RequireConfirmedInputsTlv>()
                ?? throw new SerializationException(
                    $"No serializer found for tlv type {nameof(RequireConfirmedInputsTlv)}");
            var requireConfirmedInputsTlv = await requireConfirmedInputsTlvSerializer.DeserializeAsync(stream);

            return new TxAckRbfMessage(payload, fundingOutputContributionTlv, requireConfirmedInputsTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing TxAckRbfMessage", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}