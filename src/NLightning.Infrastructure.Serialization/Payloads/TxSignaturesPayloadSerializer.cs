using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.Serialization.Interfaces;
using NLightning.Domain.Transactions.Constants;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Converters;
using Domain.Protocol.Payloads;
using Exceptions;

public class TxSignaturesPayloadSerializer : IPayloadSerializer<TxSignaturesPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public TxSignaturesPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }

    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not TxSignaturesPayload txSignaturesPayload)
            throw new SerializationException($"Payload is not of type {nameof(TxSignaturesPayload)}");

        // Get the ChannelId serializer
        var channelIdSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChannelId>()
         ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(txSignaturesPayload.ChannelId, stream);

        await stream.WriteAsync(txSignaturesPayload.TxId);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian((ushort)txSignaturesPayload.Witnesses.Count));

        // Get the Witness serializer
        var witnessSerializer =
            _valueObjectSerializerFactory.GetSerializer<Witness>()
         ?? throw new SerializationException($"No serializer found for value object type {nameof(Witness)}");

        foreach (var witness in txSignaturesPayload.Witnesses)
        {
            await witnessSerializer.SerializeAsync(witness, stream);
        }
    }

    public async Task<TxSignaturesPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(sizeof(ushort));

        try
        {
            // Get the ChannelId serializer
            var channelIdSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
             ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            var txId = new byte[TransactionConstants.TxIdLength];
            await stream.ReadExactlyAsync(txId);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var numWitnesses = EndianBitConverter.ToUInt16BigEndian(buffer[..sizeof(ushort)]);

            // Get the Witness serializer
            var witnessSerializer =
                _valueObjectSerializerFactory.GetSerializer<Witness>()
             ?? throw new SerializationException($"No serializer found for value object type {nameof(Witness)}");

            var witnesses = new List<Witness>(numWitnesses);
            for (var i = 0; i < numWitnesses; i++)
            {
                witnesses.Add(await witnessSerializer.DeserializeAsync(stream));
            }

            return new TxSignaturesPayload(channelId, txId, witnesses);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException($"Error deserializing {nameof(TxSignaturesPayload)}", e);
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