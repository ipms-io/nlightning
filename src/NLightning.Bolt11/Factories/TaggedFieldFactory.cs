namespace NLightning.Bolt11.Factories;

using Common.Utils;
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
            TaggedFieldTypes.PaymentHash => PaymentHashTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.RoutingInfo => RoutingInfoTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.Features => FeaturesTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.ExpiryTime => ExpiryTimeTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.FallbackAddress => FallbackAddressTaggedField.FromBitReader(bitReader, length, network),
            TaggedFieldTypes.Description => DescriptionTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.PaymentSecret => PaymentSecretTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.PayeePubKey => PayeePubKeyTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.DescriptionHash => DescriptionHashTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.MinFinalCltvExpiry => MinFinalCltvExpiryTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.Metadata => MetadataTaggedField.FromBitReader(bitReader, length),
            // Add more cases as needed for other types
            _ => throw new ArgumentException($"Unknown tagged field type: {type}", nameof(type))
        };
    }
}