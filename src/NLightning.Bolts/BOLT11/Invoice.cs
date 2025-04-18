using System.Text;
using System.Text.RegularExpressions;
using NBitcoin;
using NLightning.Common.Node;

namespace NLightning.Bolts.BOLT11;

using Common.BitUtils;
using Common.Constants;
using Common.Crypto.Hashes;
using Common.Managers;
using Common.Types;
using Constants;
using Encoders;
using Enums;
using Types;
using Types.TaggedFields;

/// <summary>
/// Represents a BOLT11 Invoice
/// </summary>
/// <remarks>
/// The invoice is a payment request that can be sent to a payer to request a payment.
/// </remarks>
public partial class Invoice
{
    #region Private Fields
    private static readonly Dictionary<string, Network> s_supportedNetworks = new()
    {
        { InvoiceConstants.PREFIX_MAINET, Network.MAIN_NET },
        { InvoiceConstants.PREFIX_TESTNET, Network.TEST_NET },
        { InvoiceConstants.PREFIX_SIGNET, Network.SIG_NET },
        { InvoiceConstants.PREFIX_REGTEST, Network.REG_TEST },
        { InvoiceConstants.PREFIX_MAINET.ToUpperInvariant(), Network.MAIN_NET },
        { InvoiceConstants.PREFIX_TESTNET.ToUpperInvariant(), Network.TEST_NET },
        { InvoiceConstants.PREFIX_SIGNET.ToUpperInvariant(), Network.SIG_NET },
        { InvoiceConstants.PREFIX_REGTEST.ToUpperInvariant(), Network.REG_TEST }
    };

    [GeneratedRegex(@"^[a-z]+((\d+)([munp])?)?$")]
    private static partial Regex AmountRegex();

    private TaggedFieldList _taggedFields { get; } = [];

    private string? _invoiceString;
    #endregion

    #region Public Properties
    /// <summary>
    /// The network the invoice is created for
    /// </summary>
    public Network Network { get; }

    /// <summary>
    /// The amount of millisatoshis the invoice is for
    /// </summary>
    public ulong AmountMilliSats { get; }

    /// <summary>
    /// The timestamp of the invoice
    /// </summary>
    /// <remarks>
    /// The timestamp is the time the invoice was created in seconds since the Unix epoch.
    /// </remarks>
    public long Timestamp { get; }

    /// <summary>
    /// The signature of the invoice
    /// </summary>
    public CompactSignature Signature { get; }

    /// <summary>
    /// The human-readable part of the invoice
    /// </summary>
    public string HumanReadablePart { get; }

    /// <summary>
    /// The amount of satoshis the invoice is for
    /// </summary>
    public ulong AmountSats => AmountMilliSats * 1_000;
    #endregion

    #region Public Properties from Tagged Fields
    /// <summary>
    /// The payment hash of the invoice
    /// </summary>
    /// <remarks>
    /// The payment hash is a 32 byte hash that is used to identify a payment
    /// </remarks>
    /// <seealso cref="NBitcoin.uint256"/>
    public uint256 PaymentHash
    {
        get
        {
            return _taggedFields.TryGet(TaggedFieldTypes.PAYMENT_HASH, out PaymentHashTaggedField? paymentHash)
                ? paymentHash!.Value
                : new uint256();
        }
        internal set
        {
            _taggedFields.Add(new PaymentHashTaggedField(value));
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
            return _taggedFields.TryGet(TaggedFieldTypes.ROUTING_INFO, out RoutingInfoTaggedField? routingInfo)
                ? routingInfo!.Value
                : null;
        }
        set
        {
            if (value != null)
            {
                _taggedFields.Add(new RoutingInfoTaggedField(value));
                value.Changed += OnTaggedFieldsChanged;
            }
        }
    }

    /// <summary>
    /// The features of the invoice
    /// </summary>
    /// <remarks>
    /// The features are used to specify the features the payer should support
    /// </remarks>
    /// <seealso cref="FeatureSet"/>
    public FeatureSet? Features
    {
        get
        {
            return _taggedFields.TryGet(TaggedFieldTypes.FEATURES, out FeaturesTaggedField? features)
                ? features!.Value
                : null;
        }
        set
        {
            if (value != null)
            {
                _taggedFields.Add(new FeaturesTaggedField(value));
                value.Changed += OnTaggedFieldsChanged;
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
    public DateTimeOffset ExpiryDate
    {
        get
        {
            return _taggedFields.TryGet(TaggedFieldTypes.EXPIRY_TIME, out ExpiryTimeTaggedField? expireIn)
                ? DateTimeOffset.FromUnixTimeSeconds(Timestamp + expireIn!.Value)
                : DateTimeOffset.FromUnixTimeSeconds(Timestamp + InvoiceConstants.DEFAULT_EXPIRATION_SECONDS);
        }
        set
        {
            var expireIn = value.ToUnixTimeSeconds() - Timestamp;
            _taggedFields.Add(new ExpiryTimeTaggedField((int)expireIn));
        }
    }

    /// <summary>
    /// The fallback addresses of the invoice
    /// </summary>
    /// <remarks>
    /// The fallback addresses are used to specify the fallback addresses the payer can use
    /// </remarks>
    /// <seealso cref="BitcoinAddress"/>
    public List<BitcoinAddress>? FallbackAddresses
    {
        get
        {
            return _taggedFields.TryGetAll(TaggedFieldTypes.FALLBACK_ADDRESS, out List<FallbackAddressTaggedField> fallbackAddress)
                ? fallbackAddress.Select(x => x.Value).ToList()
                : null;
        }
        set
        {
            if (value != null)
            {
                _taggedFields.AddRange(value.Select(x => new FallbackAddressTaggedField(x)));
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
            return _taggedFields.TryGet(TaggedFieldTypes.DESCRIPTION, out DescriptionTaggedField? description)
                ? description!.Value
                : null;
        }
        internal set
        {
            if (value != null)
            {
                _taggedFields.Add(new DescriptionTaggedField(value));
            }
        }
    }

    /// <summary>
    /// The payment secret of the invoice
    /// </summary>
    /// <remarks>
    /// The payment secret is a 32 byte secret that is used to identify a payment
    /// </remarks>
    /// <seealso cref="uint256"/>
    public uint256 PaymentSecret
    {
        get
        {
            return _taggedFields.TryGet(TaggedFieldTypes.PAYMENT_SECRET, out PaymentSecretTaggedField? paymentSecret)
                ? paymentSecret!.Value
                : new uint256();
        }
        internal set
        {
            _taggedFields.Add(new PaymentSecretTaggedField(value));
        }
    }

    /// <summary>
    /// The payee pubkey of the invoice
    /// </summary>
    /// <remarks>
    /// The payee pubkey is the pubkey of the payee
    /// </remarks>
    /// <seealso cref="PubKey"/>
    public PubKey? PayeePubKey
    {
        get
        {
            return _taggedFields.TryGet(TaggedFieldTypes.PAYEE_PUB_KEY, out PayeePubKeyTaggedField? payeePubKey)
                ? payeePubKey!.Value
                : null;
        }
        set
        {
            if (value != null)
            {
                _taggedFields.Add(new PayeePubKeyTaggedField(value));
            }
        }
    }

    /// <summary>
    /// The description hash of the invoice
    /// </summary>
    /// <remarks>
    /// The description hash is a 32 byte hash of the description
    /// </remarks>
    /// <seealso cref="uint256"/>
    public uint256? DescriptionHash
    {
        get
        {
            return _taggedFields.TryGet(TaggedFieldTypes.DESCRIPTION_HASH, out DescriptionHashTaggedField? descriptionHash)
                ? descriptionHash!.Value
                : null;
        }
        internal set
        {
            if (value != null)
            {
                _taggedFields.Add(new DescriptionHashTaggedField(value));
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
            return _taggedFields.TryGet(TaggedFieldTypes.MIN_FINAL_CLTV_EXPIRY, out MinFinalCltvExpiryTaggedField? minFinalCltvExpiry)
                ? minFinalCltvExpiry!.Value
                : null;
        }
        set
        {
            if (value.HasValue)
            {
                _taggedFields.Add(new MinFinalCltvExpiryTaggedField(value.Value));
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
            return _taggedFields.TryGet(TaggedFieldTypes.METADATA, out MetadataTaggedField? metadata)
                ? metadata!.Value
                : null;
        }
        set
        {
            if (value != null)
            {
                _taggedFields.Add(new MetadataTaggedField(value));
            }
        }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// The base constructor for the invoice
    /// </summary>
    /// <param name="amountMilliSats">The amount of millisatoshis the invoice is for</param>
    /// <param name="description">The description of the invoice</param>
    /// <param name="paymentHash">The payment hash of the invoice</param>
    /// <param name="paymentSecret">The payment secret of the invoice</param>
    /// <remarks>
    /// The invoice is created with the given amount of millisatoshis, a description, the payment hash and the payment secret.
    /// </remarks>
    /// <seealso cref="Network"/>
    public Invoice(ulong amountMilliSats, string description, uint256 paymentHash, uint256 paymentSecret)
    {
        AmountMilliSats = amountMilliSats;
        Network = ConfigManager.Instance.Network;
        HumanReadablePart = BuildHumanReadablePart();
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Signature = new CompactSignature(0, new byte[64]);

        // Set Required Fields
        PaymentHash = paymentHash;
        PaymentSecret = paymentSecret;
        Description = description;

        _taggedFields.Changed += OnTaggedFieldsChanged;
    }

    /// <summary>
    /// The base constructor for the invoice
    /// </summary>
    /// <param name="amountMilliSats">The amount of millisatoshis the invoice is for</param>
    /// <param name="descriptionHash">The description hash of the invoice</param>
    /// <param name="paymentHash">The payment hash of the invoice</param>
    /// <param name="paymentSecret">The payment secret of the invoice</param>
    /// <remarks>
    /// The invoice is created with the given amount of millisatoshis, a description hash, the payment hash and the payment secret.
    /// </remarks>
    /// <seealso cref="Network"/>
    public Invoice(ulong amountMilliSats, uint256 descriptionHash, uint256 paymentHash, uint256 paymentSecret)
    {
        AmountMilliSats = amountMilliSats;
        Network = ConfigManager.Instance.Network;
        HumanReadablePart = BuildHumanReadablePart();
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Signature = new CompactSignature(0, new byte[64]);

        // Set Required Fields
        PaymentHash = paymentHash;
        PaymentSecret = paymentSecret;
        DescriptionHash = descriptionHash;

        _taggedFields.Changed += OnTaggedFieldsChanged;
    }

    /// <summary>
    /// This constructor is used by tests
    /// </summary>
    /// <param name="network">The network the invoice is created for</param>
    /// <param name="amountMilliSats">The amount of millisatoshis the invoice is for</param>
    /// <param name="timestamp">The timestamp of the invoice</param>
    /// <remarks>
    /// The invoice is created with the given network, amount of millisatoshis and timestamp.
    /// </remarks>
    /// <seealso cref="Network"/>
    internal Invoice(Network network, ulong? amountMilliSats = 0, long? timestamp = null)
    {
        AmountMilliSats = amountMilliSats ?? 0;
        Network = network;
        HumanReadablePart = BuildHumanReadablePart();
        Timestamp = timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Signature = new CompactSignature(0, new byte[64]);

        _taggedFields.Changed += OnTaggedFieldsChanged;
    }

    /// <summary>
    /// This constructor is used by Decode
    /// </summary>
    /// <param name="invoiceString">The invoice string</param>
    /// <param name="humanReadablePart">The human-readable part of the invoice</param>
    /// <param name="network">The network the invoice is created for</param>
    /// <param name="amountMilliSats">The amount of millisatoshis the invoice is for</param>
    /// <param name="timestamp">The timestamp of the invoice</param>
    /// <param name="taggedFields">The tagged fields of the invoice</param>
    /// <param name="signature">The invoice signature</param>
    /// <remarks>
    /// The invoice is created with the given human-readable part, network, amount of millisatoshis, timestamp and tagged fields.
    /// </remarks>
    /// <seealso cref="Network"/>
    private Invoice(string invoiceString, string humanReadablePart, Network network, ulong amountMilliSats, long timestamp, TaggedFieldList taggedFields, CompactSignature signature)
    {
        _invoiceString = invoiceString;

        Network = network;
        HumanReadablePart = humanReadablePart;
        AmountMilliSats = amountMilliSats;
        Timestamp = timestamp;
        _taggedFields = taggedFields;
        Signature = signature;

        _taggedFields.Changed += OnTaggedFieldsChanged;
    }
    #endregion

    #region Static Constructors
    /// <summary>
    /// Creates a new invoice with the given amount of satoshis
    /// </summary>
    /// <param name="amountSats">The amount of satoshis the invoice is for</param>
    /// <param name="description">The description of the invoice</param>
    /// <param name="paymentHash">The payment hash of the invoice</param>
    /// <param name="paymentSecret">The payment secret of the invoice</param>
    /// <remarks>
    /// The invoice is created with the given amount of satoshis, a description, the payment hash and the payment secret.
    /// </remarks>
    /// <returns>The invoice</returns>
    public static Invoice InSatoshis(ulong amountSats, string description, uint256 paymentHash, uint256 paymentSecret)
    {
        return new Invoice(amountSats * 1_000, description, paymentHash, paymentSecret);
    }

    /// <summary>
    /// Creates a new invoice with the given amount of satoshis
    /// </summary>
    /// <param name="amountSats">The amount of satoshis the invoice is for</param>
    /// <param name="descriptionHash">The description hash of the invoice</param>
    /// <param name="paymentHash">The payment hash of the invoice</param>
    /// <param name="paymentSecret">The payment secret of the invoice</param>
    /// <remarks>
    /// The invoice is created with the given amount of satoshis, a description hash, the payment hash and the payment secret.
    /// </remarks>
    /// <returns>The invoice</returns>
    public static Invoice InSatoshis(ulong amountSats, uint256 descriptionHash, uint256 paymentHash, uint256 paymentSecret)
    {
        return new Invoice(amountSats * 1_000, descriptionHash, paymentHash, paymentSecret);
    }

    /// <summary>
    /// Decodes an invoice from a string
    /// </summary>
    /// <param name="invoiceString">The invoice string</param>
    /// <returns>The invoice</returns>
    /// <exception cref="InvoiceSerializationException">If something goes wrong in the decoding process</exception>
    public static Invoice Decode(string? invoiceString)
    {
        if (string.IsNullOrWhiteSpace(invoiceString))
            throw new InvoiceSerializationException("Invoice string was empty");

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

            var invoice = new Invoice(invoiceString, hrp, network, amount, timestamp, taggedFields, new CompactSignature(signature[^1], signature[..^1]));

            // Get pubkey from tagged fields
            if (taggedFields.TryGet(TaggedFieldTypes.PAYEE_PUB_KEY, out PayeePubKeyTaggedField? pubkeyTaggedField))
            {
                invoice.PayeePubKey = pubkeyTaggedField?.Value;
            }
            // Check Signature
            invoice.CheckSignature(data);

            return invoice;
        }
        catch (Exception e)
        {
            throw new InvoiceSerializationException("Error decoding invoice", e);
        }
    }
    #endregion

    /// <summary>
    /// Encodes the invoice to a string
    /// </summary>
    /// <returns>A string representing the invoice</returns>
    /// <exception cref="InvoiceSerializationException">If something goes wrong in the encoding process</exception>
    public string Encode()
    {
        try
        {
            // Calculate the size needed for the buffer
            var sizeInBits = 35 + (_taggedFields.CalculateSizeInBits() * 5) + (_taggedFields.Count * 15);

            // Initialize the BitWriter buffer
            var bitWriter = new BitWriter(sizeInBits);

            // Write the timestamp
            bitWriter.WriteInt64AsBits(Timestamp, 35);

            // Write the tagged fields
            _taggedFields.WriteToBitWriter(bitWriter);

            // Sign the invoice
            var compactSignature = SignInvoice(HumanReadablePart, bitWriter);
            var signature = new byte[compactSignature.Signature.Length + 1];
            compactSignature.Signature.CopyTo(signature, 0);
            signature[^1] = (byte)compactSignature.RecoveryId;

            var bech32Encoder = new Bech32Encoder(HumanReadablePart);
            _invoiceString = bech32Encoder.EncodeLightningInvoice(bitWriter, signature);

            return _invoiceString;
        }
        catch (Exception e)
        {
            throw new InvoiceSerializationException("Error encoding invoice", e);
        }
    }

    #region Overrides
    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(_invoiceString) ? Encode() : _invoiceString;
    }
    #endregion

    #region Private Methods
    private string BuildHumanReadablePart()
    {
        StringBuilder sb = new(InvoiceConstants.PREFIX);
        sb.Append(GetPrefix(Network));
        if (AmountMilliSats > 0)
        {
            ConvertMilliSatoshisToHumanReadable(AmountMilliSats, sb);
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

    private static void ConvertMilliSatoshisToHumanReadable(ulong millisatoshis, StringBuilder sb)
    {
        var btcAmount = millisatoshis / InvoiceConstants.BTC_IN_MILLISATOSHIS;

        // Start with the smallest multiplier
        var tempAmount = btcAmount * 1_000_000_000_000m; // Start with pico
        char? suffix = InvoiceConstants.MULTIPLIER_PICO;

        // Try nano
        if (millisatoshis % 10 == 0)
        {
            var nanoAmount = btcAmount * 1_000_000_000m;
            if (nanoAmount == decimal.Truncate(nanoAmount))
            {
                tempAmount = nanoAmount;
                suffix = InvoiceConstants.MULTIPLIER_NANO;
            }
        }

        // Try micro
        if (millisatoshis % 1_000 == 0)
        {
            var microAmount = btcAmount * 1_000_000m;
            if (microAmount == decimal.Truncate(microAmount))
            {
                tempAmount = microAmount;
                suffix = InvoiceConstants.MULTIPLIER_MICRO;
            }
        }

        // Try milli
        if (millisatoshis % 1_000_000 == 0)
        {
            var milliAmount = btcAmount * 1000m;
            if (milliAmount == decimal.Truncate(milliAmount))
            {
                tempAmount = milliAmount;
                suffix = InvoiceConstants.MULTIPLIER_MILLI;
            }
        }

        // Try full BTC
        if (millisatoshis % 1_000_000_000 == 0)
        {
            if (btcAmount == decimal.Truncate(btcAmount))
            {
                tempAmount = btcAmount;
                suffix = null;
            }
        }

        sb.Append(tempAmount.ToString("F0").TrimEnd('.'));
        sb.Append(suffix);
    }

    private static ulong ConvertHumanReadableToMilliSatoshis(string humanReadablePart)
    {
        var match = AmountRegex().Match(humanReadablePart);
        if (!match.Success)
        {
            throw new ArgumentException("Invalid amount format in invoice", nameof(humanReadablePart));
        }

        var amountString = match.Groups[2].Value;
        var multiplier = match.Groups[3].Value;
        var millisatoshis = 0ul;
        if (!ulong.TryParse(amountString, out var amount))
        {
            return millisatoshis;
        }

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

        return millisatoshis;
    }

    private void CheckSignature(byte[] data)
    {
        // Assemble the message (hrp + data)
        var message = new byte[HumanReadablePart.Length + data.Length];
        Encoding.UTF8.GetBytes(HumanReadablePart).CopyTo(message, 0);
        data.CopyTo(message, HumanReadablePart.Length);

        // Get sha256 hash of the message
        var hash = new byte[CryptoConstants.SHA256_HASH_LEN];
        using var sha256 = new Sha256();
        sha256.AppendData(message);
        sha256.GetHashAndReset(hash);

        var nBitcoinHash = new uint256(hash);

        // Check if recovery is necessary
        if (PayeePubKey == null)
        {
            PayeePubKey = PubKey.RecoverCompact(nBitcoinHash, Signature);
            return;
        }

        if (NBitcoin.Crypto.ECDSASignature.TryParseFromCompact(Signature.Signature, out var ecdsa) && PayeePubKey.Verify(nBitcoinHash, ecdsa))
        {
            return;
        }

        throw new ArgumentException("Invalid signature in invoice");
    }

    private static CompactSignature SignInvoice(string hrp, BitWriter bitWriter)
    {
        // Assemble the message (hrp + data)
        var data = bitWriter.ToArray();
        var message = new byte[hrp.Length + data.Length];
        Encoding.UTF8.GetBytes(hrp).CopyTo(message, 0);
        data.CopyTo(message, hrp.Length);

        // Get sha256 hash of the message
        var hash = new byte[CryptoConstants.SHA256_HASH_LEN];
        using var sha256 = new Sha256();
        sha256.AppendData(message);
        sha256.GetHashAndReset(hash);
        var nBitcoinHash = new uint256(hash);

        // Sign the hash
        using var key = new Key(SecureKeyManager.GetPrivateKeyBytes());
        return key.SignCompact(nBitcoinHash, false);
    }

    private static Network GetNetwork(string? invoiceString)
    {
        if (string.IsNullOrWhiteSpace(invoiceString))
        {
            throw new ArgumentException("Invoice string was empty", nameof(invoiceString));
        }

        if (!s_supportedNetworks.TryGetValue(invoiceString.Substring(2, 4), out var network)
            && !s_supportedNetworks.TryGetValue(invoiceString.Substring(2, 3), out network)
            && !s_supportedNetworks.TryGetValue(invoiceString.Substring(2, 2), out network))
        {
            throw new ArgumentException("Unsupported prefix in invoice", nameof(invoiceString));
        }

        return network;
    }

    private void OnTaggedFieldsChanged(object? sender, EventArgs args)
    {
        _invoiceString = null;
    }
    #endregion
}