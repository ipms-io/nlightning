using System.Diagnostics.CodeAnalysis;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using BOLT11.Enums;

public sealed class ExpiryTimeTaggedField : BaseTaggedField<int>
{
    [SetsRequiredMembers]
    public ExpiryTimeTaggedField(BitReader buffer, int length) : base(TaggedFieldTypes.ExpiryTime, length)
    {
        Value = buffer.ReadInt32FromBits(length * 5);
    }

    protected override int Decode(byte[] data)
    {
        throw new NotImplementedException();
    }

    public override bool IsValid()
    {
        return Value >= 0;
    }
}