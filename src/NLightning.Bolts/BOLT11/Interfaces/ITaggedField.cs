namespace NLightning.Bolts.BOLT11.Interfaces;

using Enums;

/// <summary>
/// Represents a tagged field
/// </summary>
/// <remarks>
/// Tagged fields are used in the BOLT11 invoice format to add additional information to the invoice.
/// </remarks>
/// <seealso cref="TaggedFieldTypes"/>
/// <seealso cref="BaseTaggedField{T}"/>
public interface ITaggedField
{
    /// <summary>
    /// The type of the tagged field
    /// </summary>
    /// <see cref="TaggedFieldTypes"/>
    TaggedFieldTypes Type { get; }

    /// <summary>
    /// The length of the tagged field
    /// </summary>
    /// <remarks>
    /// Length is represented by 10 bits, big-endian, and has a maximum value of 1023
    /// </remarks>
    short Length { get; }

    /// <summary>
    /// The data of the tagged field
    /// </summary>
    /// <remarks>
    /// Data is represented by a byte array
    /// </remarks>
    byte[] Data { get; }

    /// <summary>
    /// Check if the tagged field is valid
    /// </summary>
    /// <returns>True if the tagged field is valid</returns>
    /// <remarks>
    /// This method should be implemented by the derived class
    /// </remarks>
    bool IsValid();

    /// <summary>
    /// Get the value of the tagged field in a readable format
    /// </summary>
    /// <returns>The value of the tagged field</returns>
    /// <remarks>
    /// This method should be implemented by the derived class
    /// </remarks>
    object GetValue();
}