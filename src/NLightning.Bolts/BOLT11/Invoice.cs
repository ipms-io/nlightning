using System.Text;
using System.Text.RegularExpressions;

namespace NLightning.Bolts.BOLT11;

using BOLT8.Constants;
using BOLT8.Hashes;
using BOLT9;
using Common;
using Common.BitUtils;
using Common.Constants;
using Constants;
using Encoders;
using Enums;
using Exceptions;
using Factories;
using Interfaces;
using Types;
using Types.TaggedFields;

/// <summary>
/// Represents a BOLT11 Invoice
/// </summary>
/// <remarks>
/// The invoice is a payment request that can be sent to a payer to request a payment.
/// </remarks>
public class Invoice
{
    #region Private Fields
    private static readonly Dictionary<string, Network> s_supportedNetworks = new()
    {
        { InvoiceConstants.PREFIX_MAINET, Network.MainNet },
        { InvoiceConstants.PREFIX_TESTNET, Network.TestNet },
        { InvoiceConstants.PREFIX_SIGNET, Network.SigNet },
        { InvoiceConstants.PREFIX_REGTEST, Network.RegTest },
        { InvoiceConstants.PREFIX_MAINET.ToUpperInvariant(), Network.MainNet },
        { InvoiceConstants.PREFIX_TESTNET.ToUpperInvariant(), Network.TestNet },
        { InvoiceConstants.PREFIX_SIGNET.ToUpperInvariant(), Network.SigNet },
        { InvoiceConstants.PREFIX_REGTEST.ToUpperInvariant(), Network.RegTest }
    };
    #endregion

    #region Public Properties
    /// <summary>
    /// The network the invoice is created for
    /// </summary>
    public Network Network { get; private set; }

    /// <summary>
    /// The amount of millisatoshis the invoice is for
    /// </summary>
    public ulong AmountMsats { get; private set; }

    /// <summary>
    /// The timestamp of the invoice
    /// </summary>
    /// <remarks>
    /// The timestamp is the time the invoice was created in seconds since the Unix epoch.
    /// </remarks>
    public long Timestamp { get; private set; }

    /// <summary>
    /// The tagged fields of the invoice
    /// </summary>
    /// <remarks>
    /// The tagged fields are used to add additional information to the invoice.
    /// </remarks>
    /// <seealso cref="TaggedFieldList"/>
    /// <seealso cref="ITaggedField"/>
    /// <seealso cref="TaggedFieldTypes"/>
    /// <seealso cref="TaggedFieldFactory"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    public TaggedFieldList TaggedFields { get; private set; } = [];

    /// <summary>
    /// The signature of the invoice
    /// </summary>
    public string Signature { get; private set; }

    /// <summary>
    /// The human readable part of the invoice
    /// </summary>
    public string HumanReadablePart { get; private set; }

    /// <summary>
    /// The amount of satoshis the invoice is for
    /// </summary>
    public ulong AmountSats => AmountMsats * 1_000;
    #endregion

    #region Public Properties from Tagged Fields
    /// <summary>
    /// The payment hash of the invoice
    /// </summary>
    /// <remarks>
    /// The payment hash is a 32 byte hash that is used to identify a payment
    /// </remarks>
    /// <seealso cref="NBitcoin.uint256"/>
    public NBitcoin.uint256 PaymentHash
    {
        get
        {
            if (TaggedFields.TryGet(TaggedFieldTypes.PaymentHash, out PaymentHashTaggedField paymentHash))
            {
                return paymentHash.Value;
            }
            return new NBitcoin.uint256();
        }
        set
        {
            TaggedFields.Add(new PaymentHashTaggedField(value));
        }
    }

    /// <summary>
    /// The Routing Information of the invoice
    /// </summary>
    /// <remarks>
    /// The routing information is used to hint about the route the payment could take
    /// </remarks>
    /// <seealso cref="RoutingInfoCollection"/>
    /// <seealso cref="RoutingInfo"/>
    public RoutingInfoCollection? RoutingInfos
    {
        get
        {
            if (TaggedFields.TryGet(TaggedFieldTypes.RoutingInfo, out RoutingInfoTaggedField routingInfo))
            {
                return routingInfo.Value;
            }
            return null;
        }
        set
        {
            if (value != null)
            {
                TaggedFields.Add(new RoutingInfoTaggedField(value));
            }
        }
    }

    /// <summary>
    /// The features of the invoice
    /// </summary>
    /// <remarks>
    /// The features are used to specify the features the payer should support
    /// </remarks>
    /// <seealso cref="BOLT9.Features"/>
    public Features? Features
    {
        get
        {
            if (TaggedFields.TryGet(TaggedFieldTypes.Features, out FeaturesTaggedField features))
            {
                return features.Value;
            }
            return null;
        }
        set
        {
            if (value != null)
            {
                TaggedFields.Add(new FeaturesTaggedField(value));
            }
        }
    }

    /// <summary>
    /// The expiry date of the invoice
    /// </summary>
    /// <remarks>
    /// The expiry date is the date the invoice expires
    /// </remarks>
    /// <seealso cref="DateTimeOffset"/>
    public DateTimeOffset? ExpiryDate
    {
        get
        {
            if (TaggedFields.TryGet(TaggedFieldTypes.ExpiryTime, out ExpiryTimeTaggedField expireIn))
            {
                return DateTimeOffset.FromUnixTimeSeconds(Timestamp + (int)expireIn.GetValue());
            }
            return DateTimeOffset.FromUnixTimeSeconds(Timestamp + InvoiceConstants.DEFAULT_EXPIRATION_SECONDS);
        }
        set
        {
            var expireIn = value?.ToUnixTimeSeconds() - Timestamp;
            if (expireIn.HasValue)
            {
                TaggedFields.Add(new ExpiryTimeTaggedField((int)expireIn.Value));
            }
            else
            {
                throw new ArgumentException("Invalid expiry date", nameof(value));
            }
        }
    }

    /// <summary>
    /// The fallback addresses of the invoice
    /// </summary>
    /// <remarks>
    /// The fallback addresses are used to specify the fallback addresses the payer can use
    /// </remarks>
    /// <seealso cref="NBitcoin.BitcoinAddress"/>
    public List<NBitcoin.BitcoinAddress>? FallbackAddresses
    {
        get
        {
            if (TaggedFields.TryGetAll(TaggedFieldTypes.FallbackAddress, out List<FallbackAddressTaggedField> fallbackAddress))
            {
                return fallbackAddress.Select(x => (NBitcoin.BitcoinAddress)x.GetValue()).ToList();
            }
            return null;
        }
        set
        {
            if (value != null)
            {
                TaggedFields.AddRange(value.Select(x => new FallbackAddressTaggedField(x)));
            }
        }
    }

    /// <summary>
    /// The description of the invoice
    /// </summary>
    /// <remarks>
    /// The description is a UTF-8 encoded string that describes, in short, the purpose of payment
    /// </remarks>
    public string? Description
    {
        get
        {
            if (TaggedFields.TryGet(TaggedFieldTypes.Description, out DescriptionTaggedField description))
            {
                return description.Value;
            }
            return null;
        }
        set
        {
            if (value != null)
            {
                TaggedFields.Add(new DescriptionTaggedField(value));
            }
        }
    }

    /// <summary>
    /// The payment secret of the invoice
    /// </summary>
    /// <remarks>
    /// The payment secret is a 32 byte secret that is used to identify a payment
    /// </remarks>
    /// <seealso cref="NBitcoin.uint256"/>
    public NBitcoin.uint256? PaymentSecret
    {
        get
        {
            if (TaggedFields.TryGet(TaggedFieldTypes.PaymentSecret, out PaymentSecretTaggedField paymentSecret))
            {
                return paymentSecret.Value;
            }
            return null;
        }
        set
        {
            if (value != null)
            {
                TaggedFields.Add(new PaymentSecretTaggedField(value));
            }
        }
    }

    /// <summary>
    /// The payee pubkey of the invoice
    /// </summary>
    /// <remarks>
    /// The payee pubkey is the pubkey of the payee
    /// </remarks>
    /// <seealso cref="NBitcoin.PubKey"/>
    public NBitcoin.PubKey? PayeePubKey
    {
        get
        {
            if (TaggedFields.TryGet(TaggedFieldTypes.PayeePubKey, out PayeePubKeyTaggedField payeePubKey))
            {
                return payeePubKey.Value;
            }
            return null;
        }
        set
        {
            if (value != null)
            {
                TaggedFields.Add(new PayeePubKeyTaggedField(value));
            }
        }
    }

    /// <summary>
    /// The description hash of the invoice
    /// </summary>
    /// <remarks>
    /// The description hash is a 32 byte hash of the description
    /// </remarks>
    /// <seealso cref="NBitcoin.uint256"/>
    public NBitcoin.uint256? DescriptionHash
    {
        get
        {
            if (TaggedFields.TryGet(TaggedFieldTypes.DescriptionHash, out DescriptionHashTaggedField descriptionHash))
            {
                return descriptionHash.Value;
            }
            return null;
        }
        set
        {
            if (value != null)
            {
                TaggedFields.Add(new DescriptionHashTaggedField(value));
            }
        }
    }

    /// <summary>
    /// The min final cltv expiry of the invoice
    /// </summary>
    /// <remarks>
    /// The min final cltv expiry is the minimum final cltv expiry the payer should use
    /// </remarks>
    public ushort? MinFinalCltvExpiry
    {
        get
        {
            if (TaggedFields.TryGet(TaggedFieldTypes.MinFinalCltvExpiry, out MinFinalCltvExpiryTaggedField minFinalCltvExpiry))
            {
                return minFinalCltvExpiry.Value;
            }
            return null;
        }
        set
        {
            if (value.HasValue)
            {
                TaggedFields.Add(new MinFinalCltvExpiryTaggedField(value.Value));
            }
        }
    }

    /// <summary>
    /// The metadata of the invoice
    /// </summary>
    /// <remarks>
    /// The metadata is used to add additional information to the invoice
    /// </remarks>
    public byte[]? Metadata
    {
        get
        {
            if (TaggedFields.TryGet(TaggedFieldTypes.Metadata, out MetadataTaggedField metadata))
            {
                return metadata.Value;
            }
            return null;
        }
        set
        {
            if (value != null)
            {
                TaggedFields.Add(new MetadataTaggedField(value));
            }
        }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// The base constructor for the invoice
    /// </summary>
    /// <param name="network">The network the invoice is created for</param>
    /// <param name="amountMsats">The amount of millisatoshis the invoice is for</param>
    /// <param name="timestamp">The timestamp of the invoice</param>
    /// <remarks>
    /// The invoice is created with the given network, amount of millisatoshis and timestamp.
    /// </remarks>
    /// <seealso cref="Network"/>
    public Invoice(Network network, ulong? amountMsats = 0, long? timestamp = null)
    {
        AmountMsats = amountMsats ?? 0;
        Network = network;
        HumanReadablePart = BuildHumanReadablePart();
        Timestamp = timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Signature = string.Empty;
    }

    /// <summary>
    /// The constructor for the invoice
    /// </summary>
    /// <param name="humanReadablePart">The human readable part of the invoice</param>
    /// <param name="network">The network the invoice is created for</param>
    /// <param name="amountMsats">The amount of millisatoshis the invoice is for</param>
    /// <param name="timestamp">The timestamp of the invoice</param>
    /// <param name="taggedFields">The tagged fields of the invoice</param>
    /// <remarks>
    /// The invoice is created with the given human readable part, network, amount of millisatoshis, timestamp and tagged fields.
    /// </remarks>
    /// <seealso cref="Network"/>
    public Invoice(string humanReadablePart, Network network, ulong amountMsats, long timestamp, TaggedFieldList taggedFields)
    {
        Network = network;
        HumanReadablePart = humanReadablePart;
        AmountMsats = amountMsats;
        Timestamp = timestamp;
        TaggedFields = taggedFields;
        Signature = string.Empty;
    }
    #endregion

    #region Static Constructors
    public static Invoice InSatoshis(Network network, long amountSats, long? timestamp = null)
    {
        return new Invoice(network, (ulong)amountSats * 1_000, timestamp);
    }

    public static Invoice Decode(string invoiceString)
    {
        try
        {
            Bech32Encoder.DecodeLightningInvoice(invoiceString, out var data, out var signature, out var hrp);

            var network = GetNetwork(invoiceString);
            var amount = ConvertHumanReadableToMilliSatoshis(hrp);

            // Initialize the BitReader buffer
            var bitReader = new BitReader(data);

            var timestamp = bitReader.ReadInt64FromBits(35);

            var taggedFields = TaggedFieldList.FromBitReader(bitReader);

            // TODO: Check feature bits
            // Get pubkey from tagged fields
            taggedFields.TryGet(TaggedFieldTypes.PayeePubKey, out PayeePubKeyTaggedField pubkey);
            // Check Signature
            CheckSignature(signature, hrp, data, pubkey?.Value);

            return new Invoice(hrp, network, amount, timestamp, taggedFields);
        }
        catch (Exception e)
        {
            throw new InvoiceSerializationException("Error parsing invoice", e);
        }
    }
    #endregion

    public string Encode()
    {
        // Calculate the size needed for the buffer
        var sizeInBits = 35 + TaggedFields.CalculateSizeInBits();

        // Initialize the BitWriter buffer
        var bitWriter = new BitWriter(sizeInBits);

        // Write the timestamp
        bitWriter.WriteInt64AsBits(Timestamp, 35);

        // Write the tagged fields
        TaggedFields.WriteToBitWriter(bitWriter);

        // Sign the invoice
        var signature = SignInvoice(HumanReadablePart, bitWriter.ToArray(), new NBitcoin.Key());

        var bech32 = new Bech32Encoder(HumanReadablePart);
        return bech32.EncodeLightningInvoice(bitWriter.ToArray(), signature);
    }

    #region Overrides
    public override string ToString()
    {
        return Encode();
    }
    #endregion

    #region Private Methods
    private string BuildHumanReadablePart()
    {
        StringBuilder sb = new(InvoiceConstants.PREFIX);
        sb.Append(GetPrefix(Network));
        if (AmountMsats > 0)
        {
            ConvertMilliSatoshisToHumanReadable(AmountMsats, sb);
        }
        return sb.ToString();
    }

    private static string GetPrefix(Network network)
    {
        return network.Name switch
        {
            NetworkConstants.MAINNET => InvoiceConstants.PREFIX_MAINET,
            NetworkConstants.TESTNET => InvoiceConstants.PREFIX_TESTNET,
            NetworkConstants.REGTEST => InvoiceConstants.PREFIX_REGTEST,
            NetworkConstants.SIGNET => InvoiceConstants.PREFIX_SIGNET,
            _ => throw new ArgumentException("Unsupported network type", nameof(network)),
        };
    }

    private void ConvertMilliSatoshisToHumanReadable(ulong millisatoshis, StringBuilder sb)
    {
        var btcAmount = millisatoshis / InvoiceConstants.BTC_IN_MILLISATOSHIS;

        if (btcAmount >= 1)
        {
            sb.Append(btcAmount.ToString("F0").TrimEnd('.'));
        }
        else if (btcAmount >= 0.001m)
        {
            sb.Append((btcAmount * 1_000).ToString("F0").TrimEnd('.'));
            sb.Append(InvoiceConstants.MULTIPLIER_MILLI);
        }
        else if (btcAmount >= 0.000001m)
        {
            sb.Append((btcAmount * 1_000_000).ToString("F0").TrimEnd('.'));
            sb.Append(InvoiceConstants.MULTIPLIER_MICRO);
        }
        else if (btcAmount >= 0.000000001m)
        {
            sb.Append((btcAmount * 1_000_000_000).ToString("F0").TrimEnd('.'));
            sb.Append(InvoiceConstants.MULTIPLIER_NANO);
        }
        else
        {
            // Ensure the last decimal of amount is 0 when using 'p' multiplier
            var picoAmount = millisatoshis * 10;
            sb.Append(picoAmount.ToString().TrimEnd('.'));
            sb.Append(InvoiceConstants.MULTIPLIER_PICO);
        }
    }

    private static ulong ConvertHumanReadableToMilliSatoshis(string humanReadablePart)
    {
        var amountPattern = @"^[a-z]+(\d+)?([munp]?)";
        var match = Regex.Match(humanReadablePart, amountPattern);
        if (!match.Success)
        {
            throw new ArgumentException("Invalid amount format in invoice", nameof(humanReadablePart));
        }

        var amountString = match.Groups[1].Value;
        var multiplier = match.Groups[2].Value;
        var millisatoshis = 0ul;
        if (ulong.TryParse(amountString, out var amount))
        {
            if (multiplier == "p" && amount % 10 != 0)
            {
                throw new ArgumentException("Invalid pico amount in invoice", nameof(humanReadablePart));
            }

            // Calculate the millisatoshis
            millisatoshis = multiplier switch
            {
                "m" => amount * 100_000_000,
                "u" => amount * 100_000,
                "n" => amount * 100,
                "p" => amount / 10,
                _ => amount * 100_000_000_000
            };
        }

        return millisatoshis;
    }

    private static void CheckSignature(byte[] signature, string hrp, byte[] data, NBitcoin.PubKey? pubkey)
    {
        // Assemble the message (hrp + data)
        var message = new byte[hrp.Length + data.Length];
        Encoding.UTF8.GetBytes(hrp).CopyTo(message, 0);
        data.CopyTo(message, hrp.Length);

        // Get sha256 hash of the message
        using var sha256 = new SHA256();
        sha256.AppendData(message);
        var hash = new byte[HashConstants.HASH_LEN];
        sha256.GetHashAndReset(hash);
        var nbitcoinHash = new NBitcoin.uint256(hash);

        var recoveryId = (int)signature[^1];
        signature = signature[..^1];
        var compactSignature = new NBitcoin.CompactSignature(recoveryId, signature);

        // Check if recovery is necessary
        if (pubkey == null)
        {
            pubkey = NBitcoin.PubKey.RecoverCompact(nbitcoinHash, compactSignature);
            return;
        }
        else if (NBitcoin.Crypto.ECDSASignature.TryParseFromCompact(compactSignature.Signature, out var ecdsa)
                && pubkey.Verify(nbitcoinHash, ecdsa))
        {
            return;
        }

        throw new ArgumentException("Invalid signature in invoice");
    }

    private static byte[] SignInvoice(string hrp, byte[] data, NBitcoin.Key key)
    {
        // Assemble the message (hrp + data)
        var message = new byte[hrp.Length + data.Length];
        Encoding.UTF8.GetBytes(hrp).CopyTo(message, 0);
        data.CopyTo(message, hrp.Length);

        // Get sha256 hash of the message
        using var sha256 = new SHA256();
        sha256.AppendData(message);
        var hash = new byte[HashConstants.HASH_LEN];
        sha256.GetHashAndReset(hash);
        var nbitcoinHash = new NBitcoin.uint256(hash);

        // Sign the hash
        var ecdsa = key.Sign(nbitcoinHash);
        return ecdsa.ToCompact();
    }

    private static Network GetNetwork(string invoiceString)
    {
        if (!s_supportedNetworks.TryGetValue(invoiceString.Substring(2, 4), out var network)
            && !s_supportedNetworks.TryGetValue(invoiceString.Substring(2, 3), out network)
            && !s_supportedNetworks.TryGetValue(invoiceString.Substring(2, 2), out network))
        {
            throw new ArgumentException("Unsupported prefix in invoice", nameof(invoiceString));
        }

        return network;
    }
    #endregion
}