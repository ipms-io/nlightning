using System.Buffers;
using System.Runtime.Serialization;
using NBitcoin;
using NLightning.Domain.Crypto.Constants;

namespace NLightning.Infrastructure.Serialization.Payloads.PayloadTypes;

using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.Serialization.Factories;
using Domain.Serialization.Payloads;
using Domain.ValueObjects;
using Exceptions;

public class ChannelReadyPayloadSerializer : IPayloadTypeSerializer<ChannelReadyPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public ChannelReadyPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not ChannelReadyPayload channelReadyPayload)
            throw new SerializationException($"Payload is not of type {nameof(ChannelReadyPayload)}");
        
        // Get the value object serializer
        var channelIdSerializer = 
            _valueObjectSerializerFactory.GetSerializer<ChannelId>() 
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(channelReadyPayload.ChannelId, stream);
        
        // Serialize other types
        await stream.WriteAsync(channelReadyPayload.SecondPerCommitmentPoint.ToBytes());
    }

    public async Task<ChannelReadyPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(CryptoConstants.PUBKEY_LEN);
        
        try
        {
            // Get the value object serializer
            var channelIdSerializer = 
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.PUBKEY_LEN]);
            var secondPerCommitmentPoint = new PubKey(buffer[..CryptoConstants.PUBKEY_LEN]);

            return new ChannelReadyPayload(channelId, secondPerCommitmentPoint);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing ChannelReadyPayload", e);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    async Task<IMessagePayload?> IPayloadTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}