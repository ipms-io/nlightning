using System.Buffers;
using System.Runtime.Serialization;
using NBitcoin.Crypto;
using NLightning.Domain.Serialization.Payloads;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Converters;
using Domain.Crypto.Constants;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.Serialization.Factories;
using Domain.ValueObjects;
using Exceptions;

public class CommitmentSignedPayloadSerializer : IPayloadSerializer<CommitmentSignedPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public CommitmentSignedPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }

    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not CommitmentSignedPayload commitmentSignedPayload)
            throw new SerializationException($"Payload is not of type {nameof(CommitmentSignedPayload)}");

        // Get the value object serializer
        var channelIdSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChannelId>()
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(commitmentSignedPayload.ChannelId, stream);

        // Serialize other types
        await stream.WriteAsync(commitmentSignedPayload.Signature.ToCompact());
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(commitmentSignedPayload.NumHtlcs));
        foreach (var htlcsSignature in commitmentSignedPayload.HtlcSignatures)
        {
            await stream.WriteAsync(htlcsSignature.ToCompact());
        }
    }

    public async Task<CommitmentSignedPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(CryptoConstants.MAX_SIGNATURE_SIZE);

        try
        {
            // Get the value object serializer
            var channelIdSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.MAX_SIGNATURE_SIZE]);
            if (!ECDSASignature.TryParseFromCompact(buffer[..CryptoConstants.MAX_SIGNATURE_SIZE], out var signature))
            {
                throw new Exception("Unable to parse signature");
            }

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var numHtlcs = EndianBitConverter.ToUInt16BigEndian(buffer[..sizeof(ushort)]);

            var htlcSignatures = new List<ECDSASignature>(numHtlcs);
            for (var i = 0; i < numHtlcs; i++)
            {
                await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.MAX_SIGNATURE_SIZE]);
                if (!ECDSASignature.TryParseFromCompact(buffer[..CryptoConstants.MAX_SIGNATURE_SIZE],
                                                        out var htlcSignature))
                {
                    throw new Exception("Unable to parse htcl signature");
                }

                htlcSignatures.Add(htlcSignature);
            }

            return new CommitmentSignedPayload(channelId, htlcSignatures, signature);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing CommitmentSignedPayload", e);
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