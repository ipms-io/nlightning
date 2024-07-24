using System.Diagnostics.CodeAnalysis;
using NBitcoin;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using BOLT11.Enums;

public sealed class FallbackAddressTaggedField : BaseTaggedField<BitcoinAddress>
{
    [SetsRequiredMembers]
    public FallbackAddressTaggedField(BitReader buffer, int length) : base(TaggedFieldTypes.FallbackAddress, length)
    {
        Data[0] = buffer.ReadByteFromBits(5); // 17 P2PKH, 18 P2SH
        buffer.ReadBits(Data.AsSpan()[1..], (length - 1) * 5);
        // TODO: Get current network
        if (Data[0] == 17) // P2PKH
        {
            Value = new KeyId(Data[1..]).GetAddress(Network.Main);
        }
        else if (Data[0] == 18) // P2SH
        {
            Value = new ScriptId(Data[1..]).GetAddress(Network.Main);
        }
        else
        {
            throw new ArgumentException("Invalid FallbackAddressTaggedField", nameof(buffer));
        }
    }

    protected override BitcoinAddress Decode(byte[] data)
    {
        throw new NotImplementedException();
    }

    public override bool IsValid()
    {
        return Value != null;
    }
}