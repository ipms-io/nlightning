using System.Diagnostics.CodeAnalysis;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using BOLT11.Enums;
using BOLT9;

public sealed class FeaturesTaggedField : BaseTaggedField<Features>
{
    [SetsRequiredMembers]
    public FeaturesTaggedField(BitReader buffer, int length) : base(TaggedFieldTypes.Features, length)
    {
        buffer.ReadBits(Data, length * 5);
        Value = Features.DeserializeFromBits(Data);
    }

    protected override Features Decode(byte[] data)
    {
        throw new NotImplementedException();
    }

    public override bool IsValid()
    {
        return Value != null;
    }
}