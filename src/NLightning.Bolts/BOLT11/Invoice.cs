using System.Text;
using System.Text.RegularExpressions;
using NBitcoin.DataEncoders;

namespace NLightning.Bolts.BOLT11;

using BOLT11.Constants;
using BOLT11.Enums;
using BOLT11.Factories;
using BOLT11.Interfaces;
using Bolts.BOLT8.Constants;
using Bolts.BOLT8.Hashes;
using Bolts.Exceptions;
using Common;
using Common.Constants;

/// <summary>
/// Represents a BOLT11 Invoice
/// </summary>
/// <remarks>
/// The invoice is a payment request that can be sent to a payer to request a payment.
/// </remarks>
public class Invoice
{
    private static readonly Dictionary<string, Network> s_supportedNetworks = new()
    {
        { InvoiceConstants.PREFIX_MAINET, Network.MainNet },
        { InvoiceConstants.PREFIX_TESTNET, Network.TestNet },
        { InvoiceConstants.PREFIX_SIGNET, Network.SigNet },
        { InvoiceConstants.PREFIX_REGTEST, Network.RegTest }
    };

    private readonly StringBuilder _sb = new(InvoiceConstants.PREFIX);

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
    public long Timestamp { get; private set; }

    /// <summary>
    /// The tagged fields of the invoice
    /// </summary>
    /// <remarks>
    /// The tagged fields are used to add additional information to the invoice.
    /// </remarks>
    /// <seealso cref="ITaggedField"/>
    /// <seealso cref="TaggedFieldTypes"/>
    /// <seealso cref="TaggedFieldFactory"/>
    /// <seealso cref="Types.TaggedFields.BaseTaggedField{T}"/>
    public Dictionary<TaggedFieldTypes, ITaggedField> TaggedFields { get; private set; } = [];

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

    /// <summary>
    /// The expiry date of the invoice
    /// </summary>
    public DateTimeOffset? ExpiryDate { get; private set; }

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
        _sb = new StringBuilder();
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
    public Invoice(string humanReadablePart, Network network, ulong amountMsats, long timestamp, Dictionary<TaggedFieldTypes, ITaggedField> taggedFields)
    {
        Network = network;
        HumanReadablePart = humanReadablePart;
        AmountMsats = amountMsats;
        Timestamp = timestamp;
        TaggedFields = taggedFields;
        Signature = string.Empty;
    }

    public override string ToString()
    {
        return _sb.ToString();
    }

    private string BuildHumanReadablePart()
    {
        _sb.Append(GetPrefix(Network));
        if (AmountMsats > 0)
        {
            ConvertMilliSatoshisToHumanReadable(AmountMsats);
        }
        return _sb.ToString();
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

    private void ConvertMilliSatoshisToHumanReadable(ulong millisatoshis)
    {
        var btcAmount = millisatoshis / InvoiceConstants.BTC_IN_MILLISATOSHIS;

        if (btcAmount >= 1)
        {
            _sb.Append(btcAmount.ToString("F0").TrimEnd('.'));
        }
        else if (btcAmount >= 0.001m)
        {
            _sb.Append((btcAmount * 1_000).ToString("F0").TrimEnd('.'));
            _sb.Append(InvoiceConstants.MULTIPLIER_MILLI);
        }
        else if (btcAmount >= 0.000001m)
        {
            _sb.Append((btcAmount * 1_000_000).ToString("F0").TrimEnd('.'));
            _sb.Append(InvoiceConstants.MULTIPLIER_MICRO);
        }
        else if (btcAmount >= 0.000000001m)
        {
            _sb.Append((btcAmount * 1_000_000_000).ToString("F0").TrimEnd('.'));
            _sb.Append(InvoiceConstants.MULTIPLIER_NANO);
        }
        else
        {
            // Ensure the last decimal of amount is 0 when using 'p' multiplier
            var picoAmount = millisatoshis * 10;
            _sb.Append(picoAmount.ToString().TrimEnd('.'));
            _sb.Append(InvoiceConstants.MULTIPLIER_PICO);
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

    public static Invoice InSatoshis(Network network, ulong amountSats)
    {
        return new Invoice(network, amountSats * 1_000);
    }

    public static Invoice Parse(string invoiceString)
    {
        try
        {
            // Validate the prefix
            if (!invoiceString.StartsWith(InvoiceConstants.PREFIX))
            {
                throw new ArgumentException("Missing prefix in invoice", nameof(invoiceString));
            }

            var (separatorIndex, data, signature) = GetInvoiceParts(invoiceString);
            var hrp = invoiceString[..separatorIndex];

            var network = GetNetwork(invoiceString);

            var amount = ConvertHumanReadableToMilliSatoshis(hrp);

            // Initialize the BitReader buffer
            var buffer = new BitReader(data);

            var timestamp = buffer.ReadInt64FromBits(35);

            var taggedFields = TaggedFieldsFromBytes(buffer);

            // TODO: Check feature bits
            // Get pubkey from tagged fields
            taggedFields.TryGetValue(TaggedFieldTypes.PayeePubKey, out var pubkey);
            // Check Signature
            CheckSignature(signature, hrp, data, (NBitcoin.PubKey?)pubkey?.GetValue());

            return new Invoice(hrp, network, amount, timestamp, taggedFields);
        }
        catch (Exception e)
        {
            throw new InvoiceSerializationException("Error parsing invoice", e);
        }
    }

    private static void CheckSignature(byte[] signature, string hrp, byte[] data, NBitcoin.PubKey? pubkey)
    {
        // Assemble the message (hrp + data)
        var message = new byte[hrp.Length + data.Length];
        Encoding.UTF8.GetBytes(hrp).CopyTo(message, 0);
        data.CopyTo(message, hrp.Length);

        // Get sha256 hash of the message
        var sha256 = new SHA256();
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

    private static Dictionary<TaggedFieldTypes, ITaggedField> TaggedFieldsFromBytes(BitReader buffer)
    {
        var taggedFields = new Dictionary<TaggedFieldTypes, ITaggedField>();
        while (buffer.HasMoreBits(15))
        {
            var type = (TaggedFieldTypes)buffer.ReadByteFromBits(5);
            var length = buffer.ReadInt32FromBits(10);
            if (length == 0 || !buffer.HasMoreBits(length * 5))
            {
                continue;
            }

            if (!Enum.IsDefined(typeof(TaggedFieldTypes), type))
            {
                buffer.SkipBits(length * 5);
            }
            else
            {
                try
                {
                    var taggedField = TaggedFieldFactory.CreateTaggedField(type, buffer, length);
                    if (taggedField.IsValid())
                    {
                        taggedFields.Add(type, taggedField);
                    }
                }
                catch
                {
                    // Skip for now, log latter
                }
            }
        }

        return taggedFields;
    }

    private static (int, byte[], byte[]) GetInvoiceParts(string invoiceString)
    {
        invoiceString = invoiceString.ToLowerInvariant();

        // Extract human readable part
        var separatorIndex = invoiceString.LastIndexOf(InvoiceConstants.SEPARATOR);
        if (separatorIndex == -1)
        {
            throw new ArgumentException("Invalid invoice format", nameof(invoiceString));
        }
        else if (separatorIndex == 0)
        {
            throw new ArgumentException("Missing human readable part in invoice", nameof(invoiceString));
        }
        else if (separatorIndex > 10)
        {
            throw new ArgumentException("Human readable part too long in invoice", nameof(invoiceString));
        }

        var bech32 = new Bech32Encoder(Encoding.UTF8.GetBytes(invoiceString[..separatorIndex]))
        {
            StrictLength = false
        };
        var data = bech32.DecodeDataRaw(invoiceString, out _);

        // Convert from 5 bit to 8 bit per byte
        var signature = BitUtils.ConvertBits(data[(data.Length - 104)..], 5, 8);
        data = BitUtils.ConvertBits(data[..(data.Length - 104)], 5, 8);

        return (separatorIndex, data, signature);
    }
}