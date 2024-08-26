using System.Diagnostics;

namespace NLightning.Bolts.BOLT11.Types;

using Common.BitUtils;
using Enums;
using Factories;
using Interfaces;

/// <summary>
/// A list of tagged fields
/// </summary>
internal class TaggedFieldList : List<ITaggedField>
{
    /// <summary>
    /// Add a tagged field to the list
    /// </summary>
    /// <param name="taggedField">The tagged field to add</param>
    /// <exception cref="ArgumentException">If the tagged field is not unique</exception>
    internal new void Add(ITaggedField taggedField)
    {
        // Check for uniqueness
        if (this.Any(x => x.Type.Equals(taggedField.Type)) && taggedField.Type != TaggedFieldTypes.FALLBACK_ADDRESS)
        {
            throw new ArgumentException($"TaggedFieldDictionary already contains a tagged field of type {taggedField.Type}");
        }

        switch (taggedField.Type)
        {
            case TaggedFieldTypes.DESCRIPTION when this.Any(x => x.Type.Equals(TaggedFieldTypes.DESCRIPTION_HASH)):
                throw new ArgumentException($"TaggedFieldDictionary already contains a tagged field of type {taggedField.Type}");
            case TaggedFieldTypes.DESCRIPTION_HASH when this.Any(x => x.Type.Equals(TaggedFieldTypes.DESCRIPTION)):
                throw new ArgumentException($"TaggedFieldDictionary already contains a tagged field of type {taggedField.Type}");
            default:
                base.Add(taggedField);
                break;
        }
    }

    /// <summary>
    /// Add a range of tagged fields to the list
    /// </summary>
    /// <param name="taggedFields">The tagged fields to add</param>
    internal new void AddRange(IEnumerable<ITaggedField> taggedFields)
    {
        foreach (var taggedField in taggedFields)
        {
            Add(taggedField);
        }
    }

    /// <summary>
    /// Try to get a tagged field of a specific type
    /// </summary>
    /// <param name="taggedFieldType">The type of the tagged field</param>
    /// <param name="taggedField">The tagged field</param>
    /// <typeparam name="T">The type of the tagged field</typeparam>
    /// <returns>True if the tagged field was found, false otherwise</returns>
    internal bool TryGet<T>(TaggedFieldTypes taggedFieldType, out T? taggedField) where T : ITaggedField
    {
        var value = Get<T>(taggedFieldType);
        if (value != null)
        {
            taggedField = value;
            return true;
        }

        taggedField = default!;
        return false;
    }

    /// <summary>
    /// Try to get all tagged fields of a specific type
    /// </summary>
    /// <param name="taggedFieldType">The type of the tagged field</param>
    /// <param name="taggedFieldList">A list containing the tagged fields</param>
    /// <typeparam name="T">The type of the tagged field</typeparam>
    /// <returns>True if the tagged fields were found, false otherwise</returns>
    internal bool TryGetAll<T>(TaggedFieldTypes taggedFieldType, out List<T> taggedFieldList) where T : ITaggedField
    {
        var value = GetAll<T>(taggedFieldType);
        if (value != null)
        {
            taggedFieldList = value;
            return true;
        }

        taggedFieldList = default!;
        return false;
    }

    /// <summary>
    /// Get a new TaggedFieldList from a BitReader
    /// </summary>
    /// <param name="bitReader">The BitReader to read from</param>
    /// <returns>A new TaggedFieldList</returns>
    internal static TaggedFieldList FromBitReader(BitReader bitReader)
    {
        var taggedFields = new TaggedFieldList();
        while (bitReader.HasMoreBits(15))
        {
            var type = (TaggedFieldTypes)bitReader.ReadByteFromBits(5);
            var length = bitReader.ReadInt16FromBits(10);
            if (length == 0 || !bitReader.HasMoreBits(length * 5))
            {
                continue;
            }

            if (!Enum.IsDefined(typeof(TaggedFieldTypes), type))
            {
                bitReader.SkipBits(length * 5);
            }
            else
            {
                try
                {
                    var taggedField = TaggedFieldFactory.CreateTaggedFieldFromBitReader(type, bitReader, length);
                    if (taggedField.IsValid())
                    {
                        try
                        {
                            taggedFields.Add(taggedField);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message);
                            // Skip for now, log latter
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    // Skip for now, log latter
                }
            }
        }

        return taggedFields;
    }

    /// <summary>
    /// Write the TaggedFieldList to a BitWriter
    /// </summary>
    /// <param name="bitWriter">The BitWriter to write to</param>
    internal void WriteToBitWriter(BitWriter bitWriter)
    {
        foreach (var taggedField in this)
        {
            // Write type
            bitWriter.WriteByteAsBits((byte)taggedField.Type, 5);

            // Write length
            bitWriter.WriteInt16AsBits(taggedField.Length, 10);

            taggedField.WriteToBitWriter(bitWriter);
        }
    }

    /// <summary>
    /// Calculate the size of the TaggedFieldList in bits
    /// </summary>
    /// <returns>The size of the TaggedFieldList in bits</returns>
    internal int CalculateSizeInBits()
    {
        return this.Sum(x => x.Length);
    }

    /// <summary>
    /// Get a tagged field of a specific type
    /// </summary>
    /// <param name="taggedFieldType">The type of the tagged field</param>
    /// <typeparam name="T">The type of the tagged field</typeparam>
    /// <returns>The tagged field</returns>
    private T? Get<T>(TaggedFieldTypes taggedFieldType) where T : ITaggedField
    {
        return (T?)this.FirstOrDefault(x => x.Type.Equals(taggedFieldType));
    }

    /// <summary>
    /// Get all tagged fields of a specific type
    /// </summary>
    /// <param name="taggedFieldType">The type of the tagged field</param>
    /// <typeparam name="T">The type of the tagged field</typeparam>
    /// <returns>A list containing the tagged fields</returns>
    private List<T>? GetAll<T>(TaggedFieldTypes taggedFieldType) where T : ITaggedField
    {
        var taggedFields = this.Where(x => x.Type.Equals(taggedFieldType)).ToList();
        return taggedFields.Count == 0
            ? null
            : taggedFields.Cast<T>().ToList();
    }
}