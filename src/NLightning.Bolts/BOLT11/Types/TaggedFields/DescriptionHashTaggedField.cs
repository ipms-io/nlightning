using NBitcoin;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

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
public sealed class DescriptionHashTaggedField : ITaggedField
{
    public TaggedFieldTypes Type => TaggedFieldTypes.DESCRIPTION_HASH;
    public uint256 Value { get; }
    public short Length => TaggedFieldConstants.HASH_LENGTH;

    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptionHashTaggedField"/> class.
    /// </summary>
    /// <param name="value">The Description Hash</param>
    public DescriptionHashTaggedField(uint256 value)
    {
        Value = value;
    }

    public void WriteToBitWriter(BitWriter bitWriter)
    {
        // Write type
        bitWriter.WriteByteAsBits((byte)Type, 5);

        // Write length
        bitWriter.WriteInt16AsBits(Length, 10);

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

    public object GetValue()
    {
        return Value;
    }

    public static DescriptionHashTaggedField FromBitReader(BitReader bitReader, short length)
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