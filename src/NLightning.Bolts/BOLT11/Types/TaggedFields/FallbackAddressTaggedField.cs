using System.Diagnostics.CodeAnalysis;
using NBitcoin;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using BOLT11.Enums;

/// <summary>
/// Tagged field for the fallback address
/// </summary>
/// <remarks>
/// The fallback address is a Bitcoin address that can be used to pay the invoice on-chain if the payment fails.
/// </remarks>
/// <seealso cref="BaseTaggedField{T}"/>
/// <seealso cref="TaggedFieldTypes"/>
/// <seealso cref="BitcoinAddress"/>
public sealed class FallbackAddressTaggedField : BaseTaggedField<BitcoinAddress>
{
    /// <summary>
    /// Constructor for FallbackAddressTaggedField from a BitReader and a length
    /// </summary>
    /// <param name="bitReader">The BitReader to read the data from</param>
    /// <param name="length">The length of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a FallbackAddressTaggedField from a BitReader and a length.
    /// The Value property is set to the decoded value.
    /// </remarks>
    /// <seealso cref="BitReader"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public FallbackAddressTaggedField(BitReader bitReader, short length) : base(TaggedFieldTypes.FallbackAddress, length)
    {
        Data[0] = bitReader.ReadByteFromBits(5);
        bitReader.ReadBits(Data.AsSpan()[1..], (length - 1) * 5);

        if (Data[0] == 0 && length * 5 % 8 != 0 && Data[^1] == 0)
        {
            Data = Data[..^1];
        }

        Value = Decode(Data);
    }

    /// <summary>
    /// Constructor for FallbackAddressTaggedField from a value
    /// </summary>
    /// <param name="value">The value of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a FallbackAddressTaggedField from a value.
    /// The Data property is set to the encoded value.
    /// </remarks>
    /// <seealso cref="BitcoinAddress"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public FallbackAddressTaggedField(BitcoinAddress value) : base(TaggedFieldTypes.FallbackAddress, value)
    { }

    protected override BitcoinAddress? Decode(byte[] data)
    {
        // TODO: Get current network
        if (data[0] == 0) // Witness
        {
            if (data.Length == 21) // P2WPKH
            {
                return new WitKeyId(Data[1..]).GetAddress(Network.Main);
            }
            else // P2WSH
            {
                return new WitScriptId(Data[1..]).GetAddress(Network.Main);
            }
        }
        if (Data[0] == 17) // P2PKH
        {
            return new KeyId(Data[1..]).GetAddress(Network.Main);
        }
        else if (Data[0] == 18) // P2SH
        {
            return new ScriptId(Data[1..]).GetAddress(Network.Main);
        }

        return null;
    }

    public override bool IsValid()
    {
        return Value != null;
    }

    protected override byte[] Encode(BitcoinAddress? value)
    {
        if (value == null)
        {
            return [];
        }

        var data = new byte[value.ScriptPubKey.Length + 1];

        if (value is BitcoinPubKeyAddress pubKeyAddress)
        {
            // P2PKH
            data[0] = 17;
            pubKeyAddress.ScriptPubKey.ToBytes().CopyTo(data, 1);
        }
        else if (value is BitcoinScriptAddress scriptAddress)
        {
            // P2SH
            data[0] = 18;
            scriptAddress.ScriptPubKey.ToBytes().CopyTo(data, 1);
        }

        return data;
    }
}