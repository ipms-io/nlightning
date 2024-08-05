namespace NLightning.Bolts.BOLT11.Factories;

using Common.BitUtils;
using Enums;
using Interfaces;
using Types.TaggedFields;

public static class TaggedFieldFactory
{
    public static ITaggedField CreateTaggedFieldFromBitReader(TaggedFieldTypes type, BitReader bitReader, short length)
    {
        return type switch
        {
            TaggedFieldTypes.PaymentHash => new PaymentHashTaggedField(bitReader, length),
            TaggedFieldTypes.RoutingInfo => new RoutingInfoTaggedField(bitReader, length),
            TaggedFieldTypes.Features => new FeaturesTaggedField(bitReader, length),
            TaggedFieldTypes.ExpiryTime => new ExpiryTimeTaggedField(bitReader, length),
            TaggedFieldTypes.FallbackAddress => new FallbackAddressTaggedField(bitReader, length),
            TaggedFieldTypes.Description => new DescriptionTaggedField(bitReader, length),
            TaggedFieldTypes.PaymentSecret => new PaymentSecretTaggedField(bitReader, length),
            TaggedFieldTypes.PayeePubKey => new PayeePubKeyTaggedField(bitReader, length),
            TaggedFieldTypes.DescriptionHash => new DescriptionHashTaggedField(bitReader, length),
            TaggedFieldTypes.MinFinalCltvExpiry => new MinFinalCltvExpiryTaggedField(bitReader, length),
            TaggedFieldTypes.Metadata => new MetadataTaggedField(bitReader, length),
            // Add more cases as needed for other types
            _ => throw new ArgumentException($"Unknown tagged field type: {type}", nameof(type))
        };
    }
}