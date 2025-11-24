namespace NLightning.Bolt11.Factories;

using Domain.Protocol.ValueObjects;
using Domain.Utils;
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
    /// <param name="bitcoinNetwork"> The network context for the tagged field, used for address parsing.</param>
    /// <returns>
    /// The tagged field, or <c>null</c> if the field type does not contain actual data (e.g., when a routing info field has no routing information).
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the tagged field type is unknown.</exception>
    internal static ITaggedField? CreateTaggedFieldFromBitReader(TaggedFieldTypes type, BitReader bitReader,
                                                                 short length, BitcoinNetwork bitcoinNetwork)
    {
        return type switch
        {
            TaggedFieldTypes.PaymentHash => PaymentHashTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.RoutingInfo => RoutingInfoTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.Features => FeaturesTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.ExpiryTime => ExpiryTimeTaggedField.FromBitReader(bitReader, length),
            TaggedFieldTypes.FallbackAddress => FallbackAddressTaggedField.FromBitReader(
                bitReader, length, bitcoinNetwork),
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