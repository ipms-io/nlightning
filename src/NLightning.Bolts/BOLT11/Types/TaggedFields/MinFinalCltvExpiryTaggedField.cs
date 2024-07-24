namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using System.Diagnostics.CodeAnalysis;
using BOLT11.Enums;

public sealed class MinFinalCltvExpiryTaggedField : BaseTaggedField<int>
{
    [SetsRequiredMembers]
    public MinFinalCltvExpiryTaggedField(BitReader buffer, int length) : base(TaggedFieldTypes.ExpiryTime, length)
    {
        Value = buffer.ReadInt32FromBits(length * 5);
    }

    protected override int Decode(byte[] data)
    {
        throw new NotImplementedException();
    }

    public override bool IsValid()
    {
        return Value > 0;
    }
}