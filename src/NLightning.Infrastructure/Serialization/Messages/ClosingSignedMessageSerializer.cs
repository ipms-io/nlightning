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

public class ClosingSignedMessageSerializer : IMessageSerializer<ClosingSignedMessage>
{
    private readonly IPayloadSerializerFactory _payloadSerializerFactory;
    
    public ClosingSignedMessageSerializer(IPayloadSerializerFactory payloadSerializerFactory)
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
    /// Deserialize a ClosingSignedMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized AcceptChannel2Message.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing ClosingSignedMessage</exception>
    public async Task<ClosingSignedMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await _payloadSerializerFactory.DeserializeAsync<ClosingSignedPayload>(stream);

            // Deserialize extension
            var extension = await TlvStream.DeserializeAsync(stream)
                            ?? throw new SerializationException("Required extension is missing");
            if (!extension.TryGetTlv(TlvConstants.FeeRange, out var feeRangeTlv))
            {
                throw new SerializationException("Required extension is missing");
            }

            return new ClosingSignedMessage(payload, FeeRangeTlv.FromTlv(feeRangeTlv!));
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing ClosingSignedMessage", e);
        }
    }
    async Task<IMessage> IMessageSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}