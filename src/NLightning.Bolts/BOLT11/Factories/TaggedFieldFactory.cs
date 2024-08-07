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
            TaggedFieldTypes.PAYMENT_HASH => PaymentHashTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.ROUTING_INFO => RoutingInfoTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.FEATURES => FeaturesTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.EXPIRY_TIME => ExpiryTimeTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.FALLBACK_ADDRESS => FallbackAddressTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.DESCRIPTION => DescriptionTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.PAYMENT_SECRET => PaymentSecretTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.PAYEE_PUB_KEY => PayeePubKeyTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.DESCRIPTION_HASH => DescriptionHashTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.MIN_FINAL_CLTV_EXPIRY => MinFinalCltvExpiryTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.METADATA => MetadataTaggedField.FromBitReader(bitReader, length),
            // Add more cases as needed for other types
            _ => throw new ArgumentException($"Unknown tagged field type: {type}", nameof(type))
        };
    }
}