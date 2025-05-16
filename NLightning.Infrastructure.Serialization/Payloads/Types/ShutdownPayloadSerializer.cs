using System.Buffers;
using System.Runtime.Serialization;
using NBitcoin;

namespace NLightning.Infrastructure.Serialization.Payloads.Types;

using Converters;
using Domain.Bitcoin.Constants;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.Serialization.Factories;
using Domain.Serialization.Payloads.Types;
using Domain.ValueObjects;
using Exceptions;

public class ShutdownPayloadSerializer : IPayloadTypeSerializer<ShutdownPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public ShutdownPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not ShutdownPayload shutdownPayload)
            throw new SerializationException($"Payload is not of type {nameof(ShutdownPayload)}");
        
        // Get the value object serializer
        var channelIdSerializer = 
            _valueObjectSerializerFactory.GetSerializer<ChannelId>() 
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(shutdownPayload.ChannelId, stream);
        
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(shutdownPayload.ScriptPubkeyLen));
        await stream.WriteAsync(shutdownPayload.ScriptPubkey.ToBytes());
    }

    public async Task<ShutdownPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(ScriptConstants.MAX_SCRIPT_SIZE);

        try
        {
            // Get the value object serializer
            var channelIdSerializer = 
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);
            
            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var len = EndianBitConverter.ToUInt16BigEndian(buffer[..sizeof(ushort)]);
            
            if (len > ScriptConstants.MAX_SCRIPT_SIZE)
                throw new SerializationException(
                    $"ScriptPubkey length {len} exceeds maximum size {ScriptConstants.MAX_SCRIPT_SIZE}");

            await stream.ReadExactlyAsync(buffer.AsMemory()[..len]);
            var scriptPubkey = new Script(buffer[..len]);

            return new ShutdownPayload(channelId, scriptPubkey);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException($"Error deserializing {nameof(ShutdownPayload)}", e);
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