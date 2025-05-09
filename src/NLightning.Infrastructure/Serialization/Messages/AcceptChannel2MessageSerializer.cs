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

public class AcceptChannel2MessageSerializer : IMessageTypeSerializer<AcceptChannel2Message>
{
    private readonly IPayloadSerializer _payloadSerializer;
    
    public AcceptChannel2MessageSerializer(IPayloadSerializer payloadSerializer)
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
            var payload = await _payloadSerializer.DeserializeAsync<AcceptChannel2Payload>(stream);

            // Deserialize extension
            var extension = await TlvStream.DeserializeAsync(stream);
            if (extension is null)
            {
                return new AcceptChannel2Message(payload);
            }

            var upfrontShutdownScriptTlv = extension.TryGetTlv(TlvConstants.UPFRONT_SHUTDOWN_SCRIPT, out var tlv)
                ? UpfrontShutdownScriptTlv.FromTlv(tlv!)
                : null;

            var channelTypeTlv = extension.TryGetTlv(TlvConstants.CHANNEL_TYPE, out tlv)
                ? ChannelTypeTlv.FromTlv(tlv!)
                : null;

            var requireConfirmedInputsTlv = extension.TryGetTlv(TlvConstants.REQUIRE_CONFIRMED_INPUTS, out tlv)
                ? RequireConfirmedInputsTlv.FromTlv(tlv!)
                : null;

            return new AcceptChannel2Message(payload, upfrontShutdownScriptTlv, channelTypeTlv, requireConfirmedInputsTlv);
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