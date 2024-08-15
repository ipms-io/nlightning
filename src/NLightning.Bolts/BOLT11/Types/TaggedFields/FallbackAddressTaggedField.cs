using NBitcoin;
using NLightning.Common.Managers;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using Common.BitUtils;
using Enums;
using Interfaces;

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
                data.AddRange(pubKeyAddress.Hash.ToBytes());
                Length = 33;
                break;
            case BitcoinScriptAddress scriptAddress:
                // P2SH
                data.Add(18);
                data.AddRange(scriptAddress.Hash.ToBytes());
                Length = 33;
                break;
            case BitcoinWitScriptAddress witScriptAddress:
                // P2WSH
                data.Add(0);
                data.AddRange(witScriptAddress.Hash.ToBytes());
                Length = 53;
                break;
            case BitcoinWitPubKeyAddress witPubKeyAddress:
                // P2WPKH
                data.Add(0);
                data.AddRange(witPubKeyAddress.Hash.ToBytes());
                Length = 33;
                break;
        }

        _data = [.. data];
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
        var network = ConfigManager.Instance.Network;

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