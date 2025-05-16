using NLightning.Common.Utils;

namespace NLightning.Bolt11.Factories;

using Domain.ValueObjects;
using Enums;
using Interfaces;
using Models.TaggedFields;

/// <summary>
/// Factory class to create tagged fields from a bit reader.
/// </summary>
internal static class TaggedFieldFactory
{
    /// <summary>
    /// Create a tagged field from a bit reader.
    /// </summary>
    /// <param name="type">The type of tagged field to create.</param>
    /// <param name="bitReader">The bit reader to read the tagged field from.</param>
    /// <param name="length">The length of the tagged field.</param>
    /// <returns>The tagged field.</returns>
    /// <exception cref="ArgumentException">Thrown when the tagged field type is unknown.</exception>
    internal static ITaggedField CreateTaggedFieldFromBitReader(TaggedFieldTypes type, BitReader bitReader,
                                                                short length, Network network)
    {
        return type switch
        {
            TaggedFieldTypes.PAYMENT_HASH => PaymentHashTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.ROUTING_INFO => RoutingInfoTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.FEATURES => FeaturesTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.EXPIRY_TIME => ExpiryTimeTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.FALLBACK_ADDRESS => FallbackAddressTaggedField.FromBitReader(bitReader, length, network),
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