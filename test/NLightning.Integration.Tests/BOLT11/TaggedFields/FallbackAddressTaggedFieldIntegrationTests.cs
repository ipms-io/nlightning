using System.Diagnostics.CodeAnalysis;
using NBitcoin;

namespace NLightning.Integration.Tests.BOLT11.TaggedFields;

using Bolt11.Models.TaggedFields;
using Domain.Protocol.ValueObjects;
using Domain.Utils;

[SuppressMessage("Usage", "xUnit1045:Avoid using TheoryData type arguments that might not be serializable")]
public class FallbackAddressTaggedFieldIntegrationTests
{
    private static readonly Network s_network = Network.GetNetwork(BitcoinNetwork.Mainnet) ?? Network.Main;

    public static readonly TheoryData<BitcoinAddress> AddressCases =
    [
        // P2WPKH (20-byte witness key hash)
        new WitKeyId([
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            0x10, 0x11, 0x12, 0x13
        ]).GetAddress(s_network),

        // P2WSH (32-byte witness script hash)
        new WitScriptId([
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
            0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F
        ]).GetAddress(s_network),

        // P2PKH (legacy pubkey hash)
        new KeyId([
            0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xF0, 0x01, 0x12,
            0x23, 0x34, 0x45, 0x56, 0x67, 0x78, 0x89, 0x9A,
            0xAB, 0xBC, 0xCD, 0xDE
        ]).GetAddress(s_network),

        // P2SH (legacy script hash)
        new ScriptId([
            0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10,
            0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77,
            0x88, 0x99, 0xAA, 0xBB
        ]).GetAddress(s_network)
    ];

    private static (byte[] Buffer, int FieldOffsetBits, short Length, BitcoinAddress Address) BuildBuffer(
        int prePadBits, int postPadBits, BitcoinAddress address)
    {
        var field = new FallbackAddressTaggedField(address);
        var fieldBits = field.Length * 5;
        var totalBits = prePadBits + fieldBits + postPadBits;

        using var writer = new BitWriter(totalBits);

        if (prePadBits > 0)
            writer.SkipBits(prePadBits);

        field.WriteToBitWriter(writer);

        if (postPadBits > 0)
            writer.SkipBits(postPadBits);

        return (writer.ToArray(), prePadBits, field.Length, address);
    }

    [Theory]
    [MemberData(nameof(AddressCases))]
    public void FromBitReader_Reads_From_Beginning_Of_Buffer(BitcoinAddress address)
    {
        var (buffer, _, length, expected) = BuildBuffer(prePadBits: 0, postPadBits: 7, address);
        var reader = new BitReader(buffer);

        var parsed = FallbackAddressTaggedField.FromBitReader(reader, length, BitcoinNetwork.Mainnet);

        Assert.Equal(expected.ToString(), parsed.Value.ToString());
    }

    [Theory]
    [MemberData(nameof(AddressCases))]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Unaligned(BitcoinAddress address)
    {
        var (buffer, fieldOffsetBits, length, expected) = BuildBuffer(prePadBits: 13, postPadBits: 11, address);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = FallbackAddressTaggedField.FromBitReader(reader, length, BitcoinNetwork.Mainnet);

        Assert.Equal(expected.ToString(), parsed.Value.ToString());
    }

    [Theory]
    [MemberData(nameof(AddressCases))]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Aligned(BitcoinAddress address)
    {
        var (buffer, fieldOffsetBits, length, expected) = BuildBuffer(prePadBits: 16, postPadBits: 11, address);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = FallbackAddressTaggedField.FromBitReader(reader, length, BitcoinNetwork.Mainnet);

        Assert.Equal(expected.ToString(), parsed.Value.ToString());
    }

    [Theory]
    [MemberData(nameof(AddressCases))]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Unaligned(BitcoinAddress address)
    {
        var (buffer, fieldOffsetBits, length, expected) = BuildBuffer(prePadBits: 5, postPadBits: 3, address);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = FallbackAddressTaggedField.FromBitReader(reader, length, BitcoinNetwork.Mainnet);

        Assert.Equal(expected.ToString(), parsed.Value.ToString());
    }

    [Theory]
    [MemberData(nameof(AddressCases))]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Aligned(BitcoinAddress address)
    {
        var (buffer, fieldOffsetBits, length, expected) = BuildBuffer(prePadBits: 8, postPadBits: 3, address);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = FallbackAddressTaggedField.FromBitReader(reader, length, BitcoinNetwork.Mainnet);

        Assert.Equal(expected.ToString(), parsed.Value.ToString());
    }

    [Theory]
    [MemberData(nameof(AddressCases))]
    public void FromBitReader_Reads_At_End_Of_Buffer(BitcoinAddress address)
    {
        var (buffer, fieldOffsetBits, length, expected) = BuildBuffer(prePadBits: 7, postPadBits: 0, address);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = FallbackAddressTaggedField.FromBitReader(reader, length, BitcoinNetwork.Mainnet);

        Assert.Equal(expected.ToString(), parsed.Value.ToString());
    }
}