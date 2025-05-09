using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Messages;

using Application.Interfaces.Serialization;
using Common.BitUtils;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Messages;
using Domain.Protocol.Payloads;
using Exceptions;

public class RevokeAndAckMessageSerializer : IMessageTypeSerializer<RevokeAndAckMessage>
{
    private readonly IPayloadSerializer _payloadSerializer;
    
    public RevokeAndAckMessageSerializer(IPayloadSerializer payloadSerializer)
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
    /// Deserialize a RevokeAndAckMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized RevokeAndAckMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing RevokeAndAckMessage</exception>
    public async Task<RevokeAndAckMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await _payloadSerializer.DeserializeAsync<RevokeAndAckPayload>(stream);

            return new RevokeAndAckMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing RevokeAndAckMessage", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}