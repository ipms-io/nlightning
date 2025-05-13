using System.Runtime.Serialization;
using NLightning.Common.Utils;
using NLightning.Domain.Protocol.Models;
using NLightning.Infrastructure.Protocol.Models;
using NLightning.Infrastructure.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Messages;

using Common.BitUtils;
using Domain.Protocol.Constants;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Messages;
using Domain.Protocol.Payloads;
using Domain.Protocol.Tlvs;
using Domain.ValueObjects;
using Exceptions;

public class ChannelReestablishMessageSerializer : IMessageSerializer<ChannelReestablishMessage>
{
    private readonly IPayloadSerializerFactory _payloadSerializerFactory;
    
    public ChannelReestablishMessageSerializer(IPayloadSerializerFactory payloadSerializerFactory)
    {
        _payloadSerializerFactory = payloadSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(message.Type));
        await _payloadSerializerFactory.SerializeAsync(message.Payload, stream);
        
        if (message.Extension?.Any() ?? false)
        {
            foreach (var tlv in message.Extension.GetTlvs())
            {
                await tlv.SerializeAsync(stream);
            }
        }
    }

    /// <summary>
    /// Deserialize a ChannelReestablishMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized ChannelReestablishMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing ChannelReestablishMessage</exception>
    public async Task<ChannelReestablishMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await _payloadSerializerFactory.DeserializeAsync<ChannelReestablishPayload>(stream);

            // Deserialize extension
            var extension = await TlvStream.DeserializeAsync(stream);
            if (extension is null)
            {
                return new ChannelReestablishMessage(payload);
            }

            var nextFundingTlv = extension.TryGetTlv(TlvConstants.NextFunding, out var tlv)
                ? NextFundingTlv.FromTlv(tlv!)
                : null;

            return new ChannelReestablishMessage(payload, nextFundingTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing ChannelReestablishMessage", e);
        }
    }
    async Task<IMessage> IMessageSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}