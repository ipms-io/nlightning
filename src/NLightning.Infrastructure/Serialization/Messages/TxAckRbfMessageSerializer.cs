using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Messages;

using Application.Interfaces.Serialization;
using Common.BitUtils;
using Domain.Protocol.Constants;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Messages;
using Domain.Protocol.Payloads;
using Domain.Protocol.Tlvs;
using Domain.ValueObjects;
using Exceptions;

public class TxAckRbfMessageSerializer : IMessageTypeSerializer<TxAckRbfMessage>
{
    private readonly IPayloadSerializer _payloadSerializer;
    
    public TxAckRbfMessageSerializer(IPayloadSerializer payloadSerializer)
    {
        _payloadSerializer = payloadSerializer;
    }
    
    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(message.Type));
        await _payloadSerializer.SerializeAsync(message.Payload, stream);
        
        if (message.Extension?.Any() ?? false)
        {
            foreach (var tlv in message.Extension.GetTlvs())
            {
                await tlv.SerializeAsync(stream);
            }
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
            var payload = await _payloadSerializer.DeserializeAsync<TxAckRbfPayload>(stream);

            // Deserialize extension
            var extension = await TlvStream.DeserializeAsync(stream);
            if (extension is null)
            {
                return new TxAckRbfMessage(payload);
            }

            var channelTypeTlv = extension.TryGetTlv(TlvConstants.FUNDING_OUTPUT_CONTRIBUTION, out var tlv)
                ? FundingOutputContributionTlv.FromTlv(tlv!)
                : null;

            var requireConfirmedInputsTlv = extension.TryGetTlv(TlvConstants.REQUIRE_CONFIRMED_INPUTS, out tlv)
                ? RequireConfirmedInputsTlv.FromTlv(tlv!)
                : null;

            return new TxAckRbfMessage(payload, channelTypeTlv, requireConfirmedInputsTlv);
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