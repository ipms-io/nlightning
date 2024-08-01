using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using BOLT11.Enums;

/// <summary>
/// Tagged field for the description
/// </summary>
/// <remarks>
/// The description is a UTF-8 encoded string that describes, in short, the purpose of payment
/// </remarks>
/// <seealso cref="BaseTaggedField{T}"/>
/// <seealso cref="TaggedFieldTypes"/>
public sealed class DescriptionTaggedField : BaseTaggedField<string>
{
    /// <summary>
    /// Constructor for DescriptionTaggedField from a BitReader and a length
    /// </summary>
    /// <param name="buffer">The BitReader to read the data from</param>
    /// <param name="length">The length of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a DescriptionTaggedField from a BitReader and a length.
    /// The Value property is set to the decoded value.
    /// </remarks>
    /// <seealso cref="BitReader"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public DescriptionTaggedField(BitReader buffer, short length) : base(TaggedFieldTypes.Description, length)
    {
        buffer.ReadBits(Data, length * 5);
        if (length * 5 % 8 > 0)
        {
            Data = Data[..^1];
        }
        Value = Decode(Data);
    }

    /// <summary>
    /// Constructor for DescriptionTaggedField from a value
    /// </summary>
    /// <param name="value">The value of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a DescriptionTaggedField from a value.
    /// The Data property is set to the encoded value.
    /// </remarks>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public DescriptionTaggedField(string value) : base(TaggedFieldTypes.Description, value)
    { }

    /// <inheritdoc/>
    public override bool IsValid()
    {
        return Value != null;
    }

    /// <inheritdoc/>
    /// <returns>The description as a string</returns>
    protected override string Decode(byte[] data)
    {
        return Encoding.UTF8.GetString(data);
    }

    /// <inheritdoc/>
    /// <returns>The description as a byte array</returns>
    protected override byte[] Encode(string value)
    {
        return Encoding.UTF8.GetBytes(value)[..^1];
    }
}