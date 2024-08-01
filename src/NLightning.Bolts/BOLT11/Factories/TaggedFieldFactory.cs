namespace NLightning.Bolts.BOLT11.Factories;

using Enums;
using Interfaces;
using Types.TaggedFields;

public static class TaggedFieldFactory
{
    public static ITaggedField CreateTaggedFieldFromBitReader(TaggedFieldTypes type, BitReader buffer, short length)
    {
        return type switch
        {
            TaggedFieldTypes.PaymentHash => new PaymentHashTaggedField(buffer, length),
            TaggedFieldTypes.RoutingInfo => new RoutingInfoTaggedField(buffer, length),
            TaggedFieldTypes.Features => new FeaturesTaggedField(buffer, length),
            TaggedFieldTypes.ExpiryTime => new ExpiryTimeTaggedField(buffer, length),
            TaggedFieldTypes.FallbackAddress => new FallbackAddressTaggedField(buffer, length),
            TaggedFieldTypes.Description => new DescriptionTaggedField(buffer, length),
            TaggedFieldTypes.PaymentSecret => new PaymentSecretTaggedField(buffer, length),
            TaggedFieldTypes.PayeePubKey => new PayeePubKeyTaggedField(buffer, length),
            TaggedFieldTypes.DescriptionHash => new DescriptionHashTaggedField(buffer, length),
            TaggedFieldTypes.MinFinalCltvExpiry => new MinFinalCltvExpiryTaggedField(buffer, length),
            TaggedFieldTypes.Metadata => new MetadataTaggedField(buffer, length),
            // Add more cases as needed for other types
            _ => throw new ArgumentException($"Unknown tagged field type: {type}", nameof(type))
        };
    }
}