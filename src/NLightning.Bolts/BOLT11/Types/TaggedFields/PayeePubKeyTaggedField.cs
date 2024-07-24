using System.Diagnostics.CodeAnalysis;
using NBitcoin;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using Enums;

[method: SetsRequiredMembers]
public sealed class PayeePubKeyTaggedField(BitReader buffer, int length) : BaseTaggedField<PubKey>(TaggedFieldTypes.PayeePubKey, buffer, length)
{
    protected override PubKey Decode(byte[] data)
    {
        return new PubKey(data);
    }

    public override bool IsValid()
    {
        return Data.Length == 33;
    }
}