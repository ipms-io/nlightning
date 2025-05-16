using NLightning.Common.Utils;

namespace NLightning.Bolt11.Models.TaggedFields;

using Enums;
using Interfaces;

/// <summary>
/// Tagged field for the metadata
/// </summary>
/// <remarks>
/// The metadata is a variable length field that can be used to store additional information
/// </remarks>
/// <seealso cref="ITaggedField"/>
internal sealed class MetadataTaggedField : ITaggedField
{
    public TaggedFieldTypes Type => TaggedFieldTypes.METADATA;
    internal byte[] Value { get; }
    public short Length { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataTaggedField"/> class.
    /// </summary>
    /// <param name="value">The metadata bytes</param>
    internal MetadataTaggedField(byte[] value)
    {
        Value = value;
        Length = (short)Math.Ceiling(value.Length * 8 / 5.0);
    }

    /// <inheritdoc/>
    public void WriteToBitWriter(BitWriter bitWriter)
    {
        // Write data
        bitWriter.WriteBits(Value, Length * 5);
    }

    /// <inheritdoc/>
    public bool IsValid()
    {
        return true;
    }

    /// <summary>
    /// Reads a MetadataTaggedField from a BitReader
    /// </summary>
    /// <param name="bitReader">The BitReader to read from</param>
    /// <param name="length">The length of the field</param>
    /// <returns>The MetadataTaggedField</returns>
    /// <exception cref="ArgumentException">Thrown when the length is invalid</exception>
    internal static MetadataTaggedField FromBitReader(BitReader bitReader, short length)
    {
        if (length <= 0)
        {
            throw new ArgumentException("Invalid length for MetadataTaggedField. Length must be greater than 0", nameof(length));
        }

        // Read the data from the BitReader
        var data = new byte[(length * 5 + 7) / 8];
        bitReader.ReadBits(data, length * 5);

        return new MetadataTaggedField(length * 5 % 8 > 0 ? data[..^1] : data);
    }
}