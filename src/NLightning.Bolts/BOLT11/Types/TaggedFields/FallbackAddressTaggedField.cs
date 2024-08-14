using NBitcoin;
using NLightning.Bolts.BOLT11.Interfaces;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using Common.BitUtils;
using Enums;

/// <summary>
/// Tagged field for the fallback address
/// </summary>
/// <remarks>
/// The fallback address is a Bitcoin address that can be used to pay the invoice on-chain if the payment fails.
/// </remarks>
/// <seealso cref="ITaggedField"/>
public sealed class FallbackAddressTaggedField : ITaggedField
{
    private readonly byte[] _data;

    public TaggedFieldTypes Type => TaggedFieldTypes.FALLBACK_ADDRESS;
    public BitcoinAddress Value { get; }
    public short Length { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptionTaggedField"/> class.
    /// </summary>
    /// <param name="value">The Description</param>
    public FallbackAddressTaggedField(BitcoinAddress value)
    {
        Value = value;
        var data = new List<byte>();

        switch (value)
        {
            case BitcoinPubKeyAddress pubKeyAddress:
                // P2PKH
                data.Add(17);
                data.AddRange(pubKeyAddress.ScriptPubKey.ToBytes());
                break;
            case BitcoinScriptAddress scriptAddress:
                // P2SH
                data.Add(18);
                data.AddRange(scriptAddress.ScriptPubKey.ToBytes());
                break;
            case BitcoinWitScriptAddress witScriptAddress:
                // P2WSH
                data.Add(0);
                data.AddRange(witScriptAddress.ScriptPubKey.ToBytes());
                break;
            case BitcoinWitPubKeyAddress witPubKeyAddress:
                // P2WPKH
                data.Add(0);
                data.AddRange(witPubKeyAddress.ScriptPubKey.ToBytes());
                break;
        }

        _data = data.ToArray();
        Length = (short)((_data.Length * 8 - 7) / 5);
    }

    public void WriteToBitWriter(BitWriter bitWriter)
    {
        // Write Address Type
        bitWriter.WriteByteAsBits(_data[0], 5);

        // Write data
        bitWriter.WriteBits(_data.AsSpan()[1..], (Length - 1) * 5);
    }

    /// <inheritdoc/>
    public bool IsValid()
    {
        return true;
    }

    public object GetValue()
    {
        return Value;
    }

    public static FallbackAddressTaggedField FromBitReader(BitReader bitReader, short length)
    {
        // TODO: Get network from context
        var network = Network.Main;

        // Get Address Type
        var addressType = bitReader.ReadByteFromBits(5);
        var newLength = length - 1;

        // Read address bytes
        var data = new byte[(newLength * 5 + 7) / 8];
        bitReader.ReadBits(data.AsSpan(), newLength * 5);

        if (newLength * 5 % 8 != 0 && data[^1] == 0)
        {
            data = data[..^1];
        }

        // TODO: Get current network
        BitcoinAddress address = addressType switch
        {
            // Witness P2WPKH
            0 when data.Length == 20 => new WitKeyId(data).GetAddress(network),
            // Witness P2WSH
            0 when data.Length == 32 => new WitScriptId(data).GetAddress(network),
            // P2PKH
            17 => new KeyId(data).GetAddress(network),
            // P2SH
            18 => new ScriptId(data).GetAddress(network),
            _ => throw new ArgumentException("Address is unknown or invalid.", nameof(bitReader))
        };

        return new FallbackAddressTaggedField(address);
    }
}