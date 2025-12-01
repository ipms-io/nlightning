using NBitcoin;

namespace NLightning.Integration.Tests.BOLT11.TaggedFields;

using Bolt11.Models.TaggedFields;
using Domain.Utils;

public class PayeePubKeyTaggedFieldIntegrationTests
{
    private static readonly PubKey s_knownPubKey = new(
        "03e7156ae33b0a208d0744199163177e909e80176e55d97a2f221ede0f934dd9ad");

    private static (byte[] Buffer, int FieldOffsetBits, short Length) BuildBuffer(
        int prePadBits, int postPadBits, PubKey pubKey)
    {
        var field = new PayeePubKeyTaggedField(pubKey);
        var fieldBits = field.Length * 5; // 53 * 5 = 265 bits
        var totalBits = prePadBits + fieldBits + postPadBits;

        using var writer = new BitWriter(totalBits);

        if (prePadBits > 0)
            writer.SkipBits(prePadBits);

        field.WriteToBitWriter(writer);

        if (postPadBits > 0)
            writer.SkipBits(postPadBits);

        return (writer.ToArray(), prePadBits, field.Length);
    }

    [Fact]
    public void FromBitReader_Reads_From_Beginning_Of_Buffer()
    {
        var (buffer, _, length) = BuildBuffer(prePadBits: 0, postPadBits: 7, s_knownPubKey);
        var reader = new BitReader(buffer);

        var parsed = PayeePubKeyTaggedField.FromBitReader(reader, length);

        Assert.Equal(s_knownPubKey.ToHex(), parsed.Value.ToHex());
    }

    [Fact]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Unaligned()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 13, postPadBits: 11, s_knownPubKey);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = PayeePubKeyTaggedField.FromBitReader(reader, length);

        Assert.Equal(s_knownPubKey.ToHex(), parsed.Value.ToHex());
    }

    [Fact]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Aligned()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 16, postPadBits: 11, s_knownPubKey);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = PayeePubKeyTaggedField.FromBitReader(reader, length);

        Assert.Equal(s_knownPubKey.ToHex(), parsed.Value.ToHex());
    }

    [Fact]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Unaligned()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 5, postPadBits: 3, s_knownPubKey);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = PayeePubKeyTaggedField.FromBitReader(reader, length);

        Assert.Equal(s_knownPubKey.ToHex(), parsed.Value.ToHex());
    }

    [Fact]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Aligned()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 8, postPadBits: 3, s_knownPubKey);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = PayeePubKeyTaggedField.FromBitReader(reader, length);

        Assert.Equal(s_knownPubKey.ToHex(), parsed.Value.ToHex());
    }

    [Fact]
    public void FromBitReader_Reads_At_End_Of_Buffer()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 7, postPadBits: 0, s_knownPubKey);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = PayeePubKeyTaggedField.FromBitReader(reader, length);

        Assert.Equal(s_knownPubKey.ToHex(), parsed.Value.ToHex());
    }
}