using NLightning.Common.Utils;

namespace NLightning.Bolt11.Interfaces;

using Enums;

/// <summary>
/// Represents a tagged field
/// </summary>
/// <remarks>
/// Tagged fields are used in the BOLT11 invoice format to add additional information to the invoice.
/// </remarks>
/// <seealso cref="TaggedFieldTypes"/>
internal interface ITaggedField
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
    /// Check if the tagged field is valid
    /// </summary>
    /// <returns>True if the tagged field is valid</returns>
    /// <remarks>
    /// This method should be implemented by the derived class
    /// </remarks>
    bool IsValid();

    /// <summary>
    /// Write the tagged field to a BitWriter
    /// </summary>
    /// <remarks>
    /// This method should be implemented by the derived class
    /// </remarks>
    /// <see cref="BitWriter"/>
    void WriteToBitWriter(BitWriter bitWriter);
}