using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Domain.Protocol.Interfaces;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Converters;
using Domain.Bitcoin.ValueObjects;
using Domain.Channels.ValueObjects;
using Domain.Enums;
using Domain.Money;
using Domain.Protocol.Payloads;
using Domain.Serialization.Interfaces;
using Exceptions;

public class TxAddOutputPayloadSerializer : IPayloadSerializer<TxAddOutputPayload>
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
        await stream.WriteAsync(txAddOutputPayload.Script);
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
            var script = new BitcoinScript(scriptBytes);

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

    async Task<IMessagePayload?> IPayloadSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}