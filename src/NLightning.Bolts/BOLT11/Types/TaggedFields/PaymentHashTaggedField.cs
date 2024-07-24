using System.Diagnostics.CodeAnalysis;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using BOLT11.Enums;

[method: SetsRequiredMembers]
public sealed class PaymentHashTaggedField(BitReader buffer, int length) : BaseTaggedField<string>(TaggedFieldTypes.PaymentHash, buffer, length)
{
    protected override string Decode(byte[] data)
    {
        return Convert.ToHexString(data).ToLowerInvariant();
    }

    public override bool IsValid()
    {
        return Data.Length == 32;
    }
}