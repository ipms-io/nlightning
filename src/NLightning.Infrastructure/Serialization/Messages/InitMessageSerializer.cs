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

public class InitMessageSerializer : IMessageTypeSerializer<InitMessage>
{
    private readonly IPayloadSerializer _payloadSerializer;
    
    public InitMessageSerializer(IPayloadSerializer payloadSerializer)
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
    /// Deserialize an InitMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized InitMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing InitMessage</exception>
    public async Task<InitMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await _payloadSerializer.DeserializeAsync<InitPayload>(stream);

            // Deserialize extension if available
            var extension = await TlvStream.DeserializeAsync(stream);
            if (extension is null)
            {
                return new InitMessage(payload);
            }

            var networksTlv = extension.TryGetTlv(TlvConstants.NETWORKS, out var tlv)
                ? NetworksTlv.FromTlv(tlv!)
                : null;

            return new InitMessage(payload, networksTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing InitMessage", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}