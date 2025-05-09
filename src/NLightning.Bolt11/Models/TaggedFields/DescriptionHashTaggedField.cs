using NBitcoin;

namespace NLightning.Bolt11.Models.TaggedFields;

using Common.BitUtils;
using Constants;
using Enums;
using Interfaces;

/// <summary>
/// Tagged field for the description hash
/// </summary>
/// <remarks>
/// The description hash is a 32 byte hash that is used to identify a description
/// </remarks>
/// <seealso cref="ITaggedField"/>
internal sealed class DescriptionHashTaggedField : ITaggedField
{
    public TaggedFieldTypes Type => TaggedFieldTypes.DESCRIPTION_HASH;
    internal uint256 Value { get; }
    public short Length => TaggedFieldConstants.HASH_LENGTH;

    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptionHashTaggedField"/> class.
    /// </summary>
    /// <param name="value">The Description Hash</param>
    internal DescriptionHashTaggedField(uint256 value)
    {
        Value = value;
    }

    /// <inheritdoc/>
    public void WriteToBitWriter(BitWriter bitWriter)
    {
        var data = Value.ToBytes();
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(data);
        }

        // Write data
        bitWriter.WriteBits(data, Length * 5);
    }

    /// <inheritdoc/>
    public bool IsValid()
    {
        return Value != uint256.Zero;
    }

    /// <summary>
    /// Reads a DescriptionHashTaggedField from a BitReader
    /// </summary>
    /// <param name="bitReader">The BitReader to read from</param>
    /// <param name="length">The length of the field</param>
    /// <returns>The DescriptionHashTaggedField</returns>
    /// <exception cref="ArgumentException">Thrown when the length is invalid</exception>
    internal static DescriptionHashTaggedField FromBitReader(BitReader bitReader, short length)
    {
        if (length != TaggedFieldConstants.HASH_LENGTH)
        {
            throw new ArgumentException($"Invalid length for DescriptionHashTaggedField. Expected {TaggedFieldConstants.HASH_LENGTH}, but got {length}");
        }

        // Read the data from the BitReader
        var data = new byte[(TaggedFieldConstants.HASH_LENGTH * 5 + 7) / 8];
        bitReader.ReadBits(data, length * 5);
        data = data[..^1];

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(data);
        }

        return new DescriptionHashTaggedField(new uint256(data));
    }
}