using System.Diagnostics.CodeAnalysis;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using Common.BitUtils;
using Encoders;
using Enums;
using Interfaces;

/// <summary>
/// Base class for tagged fields
/// </summary>
/// <typeparam name="T">Type of the Value property</typeparam>
/// <param name="type">Tagged Field Type</param>
/// <remarks>
/// This class is used to create tagged fields for the BOLT11 invoice format.
/// T must not be null
/// </remarks>
/// <seealso cref="ITaggedField"/>
/// <seealso cref="TaggedFieldTypes"/>
[method: SetsRequiredMembers]
public abstract class BaseTaggedField<T>(TaggedFieldTypes type) : ITaggedField
    where T : notnull
{
    /// <inheritdoc/>
    public TaggedFieldTypes Type { get; protected set; } = type;

    /// <inheritdoc/>
    public short Length => (short)Data.Length;

    /// <inheritdoc/>
    public required byte[] Data { get; set; } = [];

    public required short LengthInBits { get; set; }

    /// <summary>
    /// The value of the tagged field in a readable format
    /// </summary>
    public required T Value { get; set; } = default!;

    /// <summary>
    /// Constructor for tagged fields from a value
    /// </summary>
    /// <param name="type">The type of the tagged field</param>
    /// <param name="value">The value of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create tagged fields from a value.
    /// The Value property is set to the value parameter.
    /// </remarks>
    [SetsRequiredMembers]
    protected BaseTaggedField(TaggedFieldTypes type, T value) : this(type)
    {
        Value = value;
        Data = Encode(value);
    }

    /// <summary>
    /// Constructor for tagged fields from a BitReader and a length
    /// </summary>
    /// <param name="type">The type of the tagged field</param>
    /// <param name="bitReader">The BitReader to read the data from</param>
    /// <param name="length">The length of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create tagged fields from a BitReader and a length.
    /// The data is read from the BitReader and the Value property is set by calling the Decode method.
    /// </remarks>
    [SetsRequiredMembers]
    protected BaseTaggedField(TaggedFieldTypes type, BitReader bitReader, short length) : this(type, length)
    {
        bitReader.ReadBits(Data, length * 5);

        // Account for padding
        var hasPadding = length * 5 % 8 != 0;

        Value = Decode(hasPadding ? Data[..^1] : Data);
    }

    /// <summary>
    /// Constructor for tagged fields from a length
    /// </summary>
    /// <param name="type">The type of the tagged field</param>
    /// <param name="length">The length of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create tagged fields from a length.
    /// The Data property is set to a byte array with the length of the tagged field.
    /// </remarks>
    [SetsRequiredMembers]
    protected BaseTaggedField(TaggedFieldTypes type, short length) : this(type)
    {
        LengthInBits = length;
        Data = new byte[(length * 5 + 7) / 8];
    }

    /// <summary>
    /// Decode the data to a value
    /// </summary>
    /// <param name="data">The data to decode</param>
    /// <returns>The value of the tagged field</returns>
    /// <remarks>
    /// This method should be implemented by the derived class
    /// </remarks>
    protected abstract T Decode(byte[] data);

    /// <summary>
    /// Encode the value to data
    /// </summary>
    /// <param name="value">The value to encode</param>
    /// <returns>The data of the tagged field</returns>
    /// <remarks>
    /// This method should be implemented by the derived class
    /// </remarks>
    protected abstract byte[] Encode(T value);

    protected byte[] AccountForPaddingWhenEncoding(byte[] value)
    {
        // Convert bytes from 8 bits to 5 bits
        Bech32Encoder.ConvertBits(value, 8, 5, out var data);

        // Save the length in bits
        LengthInBits = (short)(data.Length * 5);

        // Convert back to 8 bits to account for padding
        Bech32Encoder.ConvertBits(data, 5, 8, out data);

        return data;
    }

    /// <inheritdoc/>
    public abstract bool IsValid();

    /// <inheritdoc/>
    /// <remarks>
    /// This method is implemented in the base class and returns the Value property.
    /// </remarks>
    public object GetValue() => Value;
}