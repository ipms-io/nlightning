using System.Buffers;
using System.Runtime.Serialization;
using NBitcoin;
using NLightning.Infrastructure.Converters;

namespace NLightning.Infrastructure.Serialization.Payloads.PayloadTypes;

using Domain.Enums;
using Domain.Money;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.Serialization.Factories;
using Domain.Serialization.Payloads;
using Domain.ValueObjects;
using Exceptions;

public class TxAddOutputPayloadSerializer : IPayloadTypeSerializer<TxAddOutputPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public TxAddOutputPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not TxAddOutputPayload txAddOutputPayload)
            throw new SerializationException($"Payload is not of type {nameof(TxAddOutputPayload)}");
        
        // Get the value object serializer
        var channelIdSerializer = 
            _valueObjectSerializerFactory.GetSerializer<ChannelId>() 
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(txAddOutputPayload.ChannelId, stream);
        
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(txAddOutputPayload.SerialId));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(txAddOutputPayload.Amount.Satoshi));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian((ushort)txAddOutputPayload.Script.Length));
        await stream.WriteAsync(txAddOutputPayload.Script.ToBytes());
    }

    public async Task<TxAddOutputPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(sizeof(ulong));

        try
        {
            // Get the value object serializer
            var channelIdSerializer = 
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            var bytes = new byte[8];
            await stream.ReadExactlyAsync(bytes);
            var serialId = EndianBitConverter.ToUInt64BigEndian(bytes);

            bytes = new byte[8];
            await stream.ReadExactlyAsync(bytes);
            var sats = LightningMoney.FromUnit(EndianBitConverter.ToUInt64BigEndian(bytes), LightningMoneyUnit.Satoshi);

            bytes = new byte[2];
            await stream.ReadExactlyAsync(bytes);
            var scriptLength = EndianBitConverter.ToUInt16BigEndian(bytes);

            var scriptBytes = new byte[scriptLength];
            await stream.ReadExactlyAsync(scriptBytes);
            var script = new Script(scriptBytes);

            return new TxAddOutputPayload(sats, channelId, script, serialId);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException($"Error deserializing {nameof(TxAddOutputPayload)}", e);
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