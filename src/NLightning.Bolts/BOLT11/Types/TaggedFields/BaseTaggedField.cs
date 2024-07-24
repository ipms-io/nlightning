using System.Diagnostics.CodeAnalysis;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using BOLT11.Enums;
using BOLT11.Interfaces;

public abstract class BaseTaggedField<T>(TaggedFieldTypes type, int length) : ITaggedField
    where T : notnull
{
    public TaggedFieldTypes Type { get; protected set; } = type;
    public int Length => Data.Length;
    public byte[] Data { get; protected set; } = new byte[length * 5 / 8 + (length * 5 % 8 >= 5 ? 1 : 0)];
    public required T Value { get; set; }

    [SetsRequiredMembers]
    protected BaseTaggedField(TaggedFieldTypes type, BitReader buffer, int length) : this(type, length)
    {
        buffer.ReadBits(Data, length * 5);
        Value = Decode(Data);
    }

    protected abstract T Decode(byte[] data);

    public abstract bool IsValid();

    public object GetValue() => Value;
}