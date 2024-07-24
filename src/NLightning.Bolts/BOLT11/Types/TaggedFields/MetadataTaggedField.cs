using System.Diagnostics.CodeAnalysis;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using BOLT11.Enums;

[method: SetsRequiredMembers]
public sealed class MetadataTaggedField(BitReader buffer, int length) : BaseTaggedField<string>(TaggedFieldTypes.Metadata, buffer, length)
{
    protected override string Decode(byte[] data)
    {
        return Convert.ToHexString(data).ToLowerInvariant();
    }

    public override bool IsValid()
    {
        return Value != null;
    }
}