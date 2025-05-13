using System.Runtime.Serialization;
using NLightning.Common.Utils;
using NLightning.Infrastructure.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Messages;

using Common.BitUtils;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Messages;
using Domain.Protocol.Payloads;
using Exceptions;

public class StfuMessageSerializer : IMessageSerializer<StfuMessage>
{
    private readonly IPayloadSerializerFactory _payloadSerializerFactory;
    
    public StfuMessageSerializer(IPayloadSerializerFactory payloadSerializerFactory)
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
    /// Deserialize a StfuMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized StfuMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing StfuMessage</exception>
    public async Task<StfuMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await _payloadSerializerFactory.DeserializeAsync<StfuPayload>(stream);

            return new StfuMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing StfuMessage", e);
        }
    }
    async Task<IMessage> IMessageSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}