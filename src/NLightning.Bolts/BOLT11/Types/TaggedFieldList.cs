using NLightning.Bolts.BOLT11.Enums;
using NLightning.Bolts.BOLT11.Factories;
using NLightning.Bolts.BOLT11.Interfaces;

namespace NLightning.Bolts.BOLT11.Types;

public class TaggedFieldList : List<ITaggedField>
{
    public new void Add(ITaggedField taggedField)
    {
        // Check for uniqueness
        if (this.Any(x => x.Type.Equals(taggedField.Type)) && taggedField.Type != TaggedFieldTypes.FallbackAddress)
        {
            throw new ArgumentException($"TaggedFieldDictionary already contains a tagged field of type {taggedField.Type}");
        }
        else if (taggedField.Type == TaggedFieldTypes.Description)
        {
            if (this.Any(x => x.Type.Equals(TaggedFieldTypes.DescriptionHash)))
            {
                throw new ArgumentException($"TaggedFieldDictionary already contains a tagged field of type {taggedField.Type}");
            }
        }
        else if (taggedField.Type == TaggedFieldTypes.DescriptionHash)
        {
            if (this.Any(x => x.Type.Equals(TaggedFieldTypes.Description)))
            {
                throw new ArgumentException($"TaggedFieldDictionary already contains a tagged field of type {taggedField.Type}");
            }
        }

        base.Add(taggedField);
    }

    public new void AddRange(IEnumerable<ITaggedField> taggedFields)
    {
        foreach (var taggedField in taggedFields)
        {
            Add(taggedField);
        }
    }

    public T? Get<T>(TaggedFieldTypes taggedFieldType) where T : ITaggedField
    {
        return (T?)this.FirstOrDefault(x => x.Type.Equals(taggedFieldType));
    }

    public List<T>? GetAll<T>(TaggedFieldTypes taggedFieldType) where T : ITaggedField
    {
        var taggedFields = this.Where(x => x.Type.Equals(taggedFieldType)).ToList();
        if (taggedFields.Count == 0)
        {
            return null;
        }

        return taggedFields.Cast<T>().ToList();
    }

    public bool TryGet<T>(TaggedFieldTypes taggedFieldType, out T taggedField) where T : ITaggedField
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

    public bool TryGetAll<T>(TaggedFieldTypes taggedFieldType, out List<T> taggedField) where T : ITaggedField
    {
        var value = GetAll<T>(taggedFieldType);
        if (value != null)
        {
            taggedField = value;
            return true;
        }

        taggedField = default!;
        return false;
    }

    public static TaggedFieldList FromBitReader(BitReader bitReader)
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
                        catch
                        {
                            // Skip for now, log latter
                        }
                    }
                }
                catch
                {
                    // Skip for now, log latter
                }
            }
        }

        return taggedFields;
    }
}