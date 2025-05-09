using System.Text;

namespace NLightning.Bolt11.Models.TaggedFields;

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
internal sealed class DescriptionTaggedField : ITaggedField
{
    private readonly byte[] _data;

    public TaggedFieldTypes Type => TaggedFieldTypes.DESCRIPTION;
    internal string Value { get; }
    public short Length { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptionTaggedField"/> class.
    /// </summary>
    /// <param name="value">The Description</param>
    internal DescriptionTaggedField(string value)
    {
        Value = value;

        if (string.IsNullOrEmpty(value))
        {
            _data = [];
            Length = 0;
        }
        else
        {
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
    }

    /// <inheritdoc/>
    public void WriteToBitWriter(BitWriter bitWriter)
    {
        if (Length == 0)
        {
            return;
        }

        // Write data
        bitWriter.WriteBits(_data, Length * 5);
    }

    /// <inheritdoc/>
    public bool IsValid()
    {
        return true;
    }

    /// <summary>
    /// Reads a DescriptionTaggedField from a BitReader
    /// </summary>
    /// <param name="bitReader">The BitReader to read from</param>
    /// <param name="length">The length of the field</param>
    /// <returns>The DescriptionTaggedField</returns>
    /// <exception cref="ArgumentException">Thrown when the length is invalid</exception>
    internal static DescriptionTaggedField FromBitReader(BitReader bitReader, short length)
    {
        switch (length)
        {
            case < 0:
                throw new ArgumentException("Invalid length for DescriptionTaggedField. Length must be greater or equal to 0", nameof(length));
            case 0:
                return new DescriptionTaggedField(string.Empty);
        }

        // Read the data from the BitReader
        var data = new byte[(length * 5 + 7) / 8];
        bitReader.ReadBits(data, length * 5);

        return new DescriptionTaggedField(Encoding.UTF8.GetString(length * 5 % 8 > 0 ? data[..^1] : data));
    }
}