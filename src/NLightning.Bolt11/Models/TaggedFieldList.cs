using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace NLightning.Bolt11.Models;

using Domain.Protocol.ValueObjects;
using Domain.Utils;
using Enums;
using Factories;
using Interfaces;

/// <summary>
/// A list of tagged fields
/// </summary>
internal class TaggedFieldList : List<ITaggedField>
{
    private bool _shouldInvokeChangedEvent = true;
    public event EventHandler? Changed;

    /// <summary>
    /// Add a tagged field to the list
    /// </summary>
    /// <param name="taggedField">The tagged field to add</param>
    /// <exception cref="ArgumentException">If the tagged field is not unique</exception>
    internal new void Add(ITaggedField taggedField)
    {
        // Individual field validation
        if (!taggedField.IsValid())
            throw new ArgumentException($"Invalid {taggedField.Type} field: field validation failed");

        // Check for uniqueness
        if (this.Any(x => x.Type.Equals(taggedField.Type)) && taggedField.Type != TaggedFieldTypes.FallbackAddress)
            throw new ArgumentException(
                $"TaggedFieldDictionary already contains a tagged field of type {taggedField.Type}");

        // Mutual exclusivity validation
        if (taggedField.Type == TaggedFieldTypes.Description
         && this.Any(x => x.Type.Equals(TaggedFieldTypes.DescriptionHash)))
            throw new ArgumentException(
                $"TaggedFieldDictionary already contains a tagged field of type {taggedField.Type}");

        if (taggedField.Type == TaggedFieldTypes.DescriptionHash
         && this.Any(x => x.Type.Equals(TaggedFieldTypes.Description)))
            throw new ArgumentException(
                $"TaggedFieldDictionary already contains a tagged field of type {taggedField.Type}");

        // Add the tagged field
        base.Add(taggedField);
        if (_shouldInvokeChangedEvent)
            OnChanged();
    }

    /// <summary>
    /// Add a range of tagged fields to the list
    /// </summary>
    /// <param name="taggedFields">The tagged fields to add</param>
    internal new void AddRange(IEnumerable<ITaggedField> taggedFields)
    {
        _shouldInvokeChangedEvent = false;

        foreach (var taggedField in taggedFields)
            Add(taggedField);

        _shouldInvokeChangedEvent = true;
        OnChanged();
    }

    internal new bool Remove(ITaggedField item)
    {
        if (!base.Remove(item))
            return false;

        OnChanged();
        return true;
    }

    internal new void RemoveAt(int index)
    {
        base.RemoveAt(index);
        OnChanged();
    }

    internal new int RemoveAll(Predicate<ITaggedField> match)
    {
        var removed = base.RemoveAll(match);
        if (removed > 0)
            OnChanged();

        return removed;
    }

    internal new void RemoveRange(int index, int count)
    {
        base.RemoveRange(index, count);
        OnChanged();
    }

    /// <summary>
    /// Attempts to retrieve a tagged field of the specified type from the list.
    /// </summary>
    /// <typeparam name="T">The type of the tagged field to retrieve.</typeparam>
    /// <param name="taggedFieldType">The type of the tagged field being searched for.</param>
    /// <param name="taggedField">The output parameter to hold the retrieved tagged field if found; otherwise, the default value for the type.</param>
    /// <returns>True if the tagged field of the specified type is found; otherwise, false.</returns>
    internal bool TryGet<T>(TaggedFieldTypes taggedFieldType, [MaybeNullWhen(false)] out T taggedField)
        where T : ITaggedField
    {
        var value = Get<T>(taggedFieldType);
        if (value != null)
        {
            taggedField = value;
            return true;
        }

        taggedField = default;
        return false;
    }

    /// <summary>
    /// Try to get all tagged fields of a specific type
    /// </summary>
    /// <param name="taggedFieldType">The type of the tagged field</param>
    /// <param name="taggedFieldList">A list containing the tagged fields</param>
    /// <typeparam name="T">The type of the tagged field</typeparam>
    /// <returns>True if the tagged fields were found, false otherwise</returns>
    internal bool TryGetAll<T>(TaggedFieldTypes taggedFieldType, [MaybeNullWhen(false)] out List<T> taggedFieldList)
        where T : ITaggedField
    {
        var value = GetAll<T>(taggedFieldType);
        if (value != null)
        {
            taggedFieldList = value;
            return true;
        }

        taggedFieldList = null;
        return false;
    }

    /// <summary>
    /// Get a new TaggedFieldList from a BitReader
    /// </summary>
    /// <param name="bitReader">The BitReader to read from</param>
    /// <param name="bitcoinNetwork">The network type</param>
    /// <returns>A new TaggedFieldList</returns>
    internal static TaggedFieldList FromBitReader(BitReader bitReader, BitcoinNetwork bitcoinNetwork)
    {
        var taggedFields = new TaggedFieldList();
        while (bitReader.HasMoreBits(15))
        {
            var type = (TaggedFieldTypes)bitReader.ReadByteFromBits(5);
            var length = bitReader.ReadInt16FromBits(10);
            if (length == 0 || !bitReader.HasMoreBits(length * 5))
                continue;

            if (!Enum.IsDefined(typeof(TaggedFieldTypes), type))
            {
                bitReader.SkipBits(length * 5);
            }
            else
            {
                try
                {
                    var taggedField =
                        TaggedFieldFactory.CreateTaggedFieldFromBitReader(type, bitReader, length, bitcoinNetwork);

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

    private void OnChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }
}