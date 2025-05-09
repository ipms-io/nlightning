using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Messages;

using Application.Interfaces.Serialization;
using Common.BitUtils;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Messages;
using Domain.Protocol.Payloads;
using Exceptions;

public class TxAbortMessageSerializer : IMessageTypeSerializer<TxAbortMessage>
{
    private readonly IPayloadSerializer _payloadSerializer;
    
    public TxAbortMessageSerializer(IPayloadSerializer payloadSerializer)
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
    /// Deserialize a TxAbortMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxAbortMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing TxAbortMessage</exception>
    public async Task<TxAbortMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await _payloadSerializer.DeserializeAsync<TxAbortPayload>(stream);

            return new TxAbortMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing TxAbortMessage", e);
        }
    }
    async Task<IMessage> IMessageTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}