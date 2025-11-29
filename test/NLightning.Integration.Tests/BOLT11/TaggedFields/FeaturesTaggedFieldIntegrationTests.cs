namespace NLightning.Integration.Tests.BOLT11.TaggedFields;

using Bolt11.Models.TaggedFields;
using Domain.Enums;
using Domain.Node;
using Domain.Utils;

public class FeaturesTaggedFieldIntegrationTests
{
    private static FeatureSet BuildKnownFeatures()
    {
        var fs = new FeatureSet();

        // Keep a minimal, dependency-safe subset to avoid negotiation/compatibility pitfalls
        // - GossipQueries (optional)
        fs.SetFeature(Feature.GossipQueries, isCompulsory: false, isSet: true);

        // - PaymentSecret (optional) so that BasicMpp optional can be satisfied
        fs.SetFeature(Feature.PaymentSecret, isCompulsory: false, isSet: true);

        // - BasicMpp (optional) depends on PaymentSecret
        fs.SetFeature(Feature.BasicMpp, isCompulsory: false, isSet: true);

        // Ensure a higher bit is set too: OptionOnionMessages (optional @ 39)
        fs.SetFeature(Feature.OptionOnionMessages, isCompulsory: false, isSet: true);

        return fs;
    }

    private static (byte[] Buffer, int FieldOffsetBits, short Length) BuildBuffer(
        int prePadBits, int postPadBits, FeatureSet features)
    {
        var field = new FeaturesTaggedField(features);
        var fieldBits = field.Length * 5;
        var totalBits = prePadBits + fieldBits + postPadBits;

        using var writer = new BitWriter(totalBits);

        if (prePadBits > 0)
            writer.SkipBits(prePadBits);

        field.WriteToBitWriter(writer);

        if (postPadBits > 0)
            writer.SkipBits(postPadBits);

        return (writer.ToArray(), prePadBits, field.Length);
    }

    private static void AssertFeatureSetContainsExpectedFeatures(FeatureSet actual)
    {
        // Spot-check some features we toggled explicitly; these must be present after the round-trip
        Assert.True(actual.IsFeatureSet(Feature.GossipQueries));
        // Dependencies: VarOnionOptin should be set by default in FeatureSet constructor
        Assert.True(actual.IsFeatureSet(Feature.VarOnionOptin));

        // We set PaymentSecret as optional here
        Assert.True(actual.IsFeatureSet(Feature.PaymentSecret));
        Assert.True(actual.IsFeatureSet(Feature.BasicMpp));
        Assert.True(actual.IsFeatureSet(Feature.OptionOnionMessages));
    }

    [Fact]
    public void FromBitReader_Reads_From_Beginning_Of_Buffer()
    {
        var features = BuildKnownFeatures();
        var (buffer, _, length) = BuildBuffer(prePadBits: 0, postPadBits: 7, features);
        var reader = new BitReader(buffer);

        var parsed = FeaturesTaggedField.FromBitReader(reader, length);

        AssertFeatureSetContainsExpectedFeatures(parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Unaligned()
    {
        var features = BuildKnownFeatures();
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 13, postPadBits: 11, features);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = FeaturesTaggedField.FromBitReader(reader, length);

        AssertFeatureSetContainsExpectedFeatures(parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Aligned()
    {
        var features = BuildKnownFeatures();
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 16, postPadBits: 11, features);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = FeaturesTaggedField.FromBitReader(reader, length);

        AssertFeatureSetContainsExpectedFeatures(parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Unaligned()
    {
        var features = BuildKnownFeatures();
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 5, postPadBits: 3, features);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = FeaturesTaggedField.FromBitReader(reader, length);

        AssertFeatureSetContainsExpectedFeatures(parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Aligned()
    {
        var features = BuildKnownFeatures();
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 8, postPadBits: 3, features);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = FeaturesTaggedField.FromBitReader(reader, length);

        AssertFeatureSetContainsExpectedFeatures(parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_At_End_Of_Buffer()
    {
        var features = BuildKnownFeatures();
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 7, postPadBits: 0, features);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = FeaturesTaggedField.FromBitReader(reader, length);

        AssertFeatureSetContainsExpectedFeatures(parsed.Value);
    }
}