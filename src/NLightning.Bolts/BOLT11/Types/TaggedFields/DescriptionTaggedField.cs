using System.Text;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using Common.BitUtils;
using Enums;
using Interfaces;

/// <summary>
/// Tagged field for the description
/// </summary>
/// <remarks>
/// The description is a UTF-8 encoded string that describes, in short, the purpose of payment
/// </remarks>
/// <seealso cref="ITaggedField"/>
public sealed class DescriptionTaggedField : ITaggedField
{
    private readonly byte[] _data;

    public TaggedFieldTypes Type => TaggedFieldTypes.DESCRIPTION;
    public string Value { get; }
    public short Length { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptionTaggedField"/> class.
    /// </summary>
    /// <param name="value">The Description</param>
    public DescriptionTaggedField(string value)
    {
        Value = value;

        // Add Padding if needed
        var data = Encoding.UTF8.GetBytes(Value);
        var bitLength = data.Length * 8;
        var totalBits = bitLength + (5 - bitLength % 5) % 5;
        if (totalBits != bitLength)
        {
            _data = new byte[data.Length + 1];
            Array.Copy(data, _data, data.Length);
        }
        else
        {
            _data = data;
        }

        Length = (short)(totalBits / 5);
    }

    public void WriteToBitWriter(BitWriter bitWriter)
    {
        // Write type
        bitWriter.WriteByteAsBits((byte)Type, 5);

        // Write length
        bitWriter.WriteInt16AsBits(Length, 10);

        // Write data
        bitWriter.WriteBits(_data, Length * 5);
    }

    /// <inheritdoc/>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Value);
    }

    public object GetValue()
    {
        return Value;
    }

    public static DescriptionTaggedField FromBitReader(BitReader bitReader, short length)
    {
        if (length <= 0)
        {
            throw new ArgumentException("Invalid length for DescriptionTaggedField. Length must be greater than 0", nameof(length));
        }

        // Read the data from the BitReader
        var data = new byte[(length * 5 + 7) / 8];
        bitReader.ReadBits(data, length * 5);

        return new DescriptionTaggedField(Encoding.UTF8.GetString(length * 5 % 8 > 0 ? data[..^1] : data));
    }
}