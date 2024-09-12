using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace NLightning.Bolts;

using BOLT9;
using Common.Constants;
using Common.Crypto.Functions;
using Common.Crypto.Primitives;
using Common.TLVs;

/// <summary>
/// Represents the options for a node.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class NodeOptions
{
    #region Network
    /// <summary>
    /// Global network timeout.
    /// </summary>
    /// <remarks>
    /// The global network timeout is used for all network operations.
    /// </remarks>
    public TimeSpan NetworkTimeout = TimeSpan.FromSeconds(15);
    #endregion

    #region Crypto
    /// <summary>
    /// The key pair of the node.
    /// </summary>
    /// <remarks>
    /// The key pair is used to sign messages and create the node id.
    /// </remarks>
    internal KeyPair KeyPair;
    #endregion

    #region Features
    /// <summary>
    /// Enable data loss protection.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableDataLossProtect;

    /// <summary>
    /// Enable initial routing sync.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableInitialRoutingSync;

    /// <summary>
    /// Enable upfront shutdown script.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableUpfrontShutdownScript;

    /// <summary>
    /// Enable gossip queries.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableGossipQueries;

    /// <summary>
    /// Enable expanded gossip queries.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableExpandedGossipQueries;

    /// <summary>
    /// Enable static remote key.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableStaticRemoteKey;

    /// <summary>
    /// Enable payment secret.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnablePaymentSecret;

    /// <summary>
    /// Enable basic MPP.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableBasicMpp;

    /// <summary>
    /// Enable large channels.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableLargeChannels;

    /// <summary>
    /// Enable anchor outputs.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableAnchorOutputs;

    /// <summary>
    /// Enable zero fee anchor tx.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableZeroFeeAnchorTx;

    /// <summary>
    /// Enable route blinding.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableRouteBlinding;

    /// <summary>
    /// Enable beyond segwit shutdown.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableBeyondSegwitShutdown;

    /// <summary>
    /// Enable dual fund.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableDualFund;

    /// <summary>
    /// Enable onion messages.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableOnionMessages;

    /// <summary>
    /// Enable channel type.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableChannelType;

    /// <summary>
    /// Enable scid alias.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableScidAlias;

    /// <summary>
    /// Enable payment metadata.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnablePaymentMetadata;

    /// <summary>
    /// Enable zero conf.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableZeroConf;
    #endregion

    #region InitExtension
    /// <summary>
    /// The chain hashes of the node.
    /// </summary>
    /// <remarks>
    /// Initialized as Mainnet if not set.
    /// </remarks>
    public IEnumerable<ChainHash> ChainHashes = [];

    /// <summary>
    /// The remote address of the node.
    /// </summary>
    /// <remarks>
    /// This is used to connect to our node.
    /// </remarks>
    public IPAddress? RemoteAddress = null;
    #endregion

    /// <summary>
    /// Initializes a new instance of the NodeOptions class.
    /// </summary>
    /// <remarks>
    /// The key pair is generated automatically.
    /// </remarks>
    public NodeOptions()
    {
        KeyPair = new Ecdh().GenerateKeyPair();
    }

    /// <summary>
    /// Initializes a new instance of the NodeOptions class.
    /// </summary>
    /// <param name="p">The private key of the node.</param>
    /// <remarks>
    /// The key pair is generated from the private key.
    /// </remarks>
    public NodeOptions(NBitcoin.Key p)
    {
        KeyPair = new KeyPair(p);
    }

    /// <summary>
    /// Get Features set for the node.
    /// </summary>
    /// <returns>The features set for the node.</returns>
    /// <remarks>
    /// All features set as OPTIONAL.
    /// </remarks>
    internal Features GetNodeFeatures()
    {
        var features = new Features();

        if (EnableDataLossProtect)
        {
            features.SetFeature(Feature.OPTION_DATA_LOSS_PROTECT, false);
        }

        if (EnableInitialRoutingSync)
        {
            features.SetFeature(Feature.INITIAL_ROUTING_SYNC, false);
        }

        if (EnableUpfrontShutdownScript)
        {
            features.SetFeature(Feature.OPTION_UPFRONT_SHUTDOWN_SCRIPT, false);
        }

        if (EnableGossipQueries)
        {
            features.SetFeature(Feature.GOSSIP_QUERIES, false);
        }

        if (EnableExpandedGossipQueries)
        {
            features.SetFeature(Feature.GOSSIP_QUERIES_EX, false);
        }

        if (EnableStaticRemoteKey)
        {
            features.SetFeature(Feature.OPTION_STATIC_REMOTE_KEY, false);
        }

        if (EnablePaymentSecret)
        {
            features.SetFeature(Feature.PAYMENT_SECRET, false);
        }

        if (EnableBasicMpp)
        {
            features.SetFeature(Feature.BASIC_MPP, false);
        }

        if (EnableLargeChannels)
        {
            features.SetFeature(Feature.OPTION_SUPPORT_LARGE_CHANNEL, false);
        }

        if (EnableAnchorOutputs)
        {
            features.SetFeature(Feature.OPTION_ANCHOR_OUTPUTS, false);
        }

        if (EnableZeroFeeAnchorTx)
        {
            features.SetFeature(Feature.OPTION_ANCHORS_ZERO_FEE_HTLC_TX, false);
        }

        if (EnableRouteBlinding)
        {
            features.SetFeature(Feature.OPTION_ROUTE_BLINDING, false);
        }

        if (EnableBeyondSegwitShutdown)
        {
            features.SetFeature(Feature.OPTION_SHUTDOWN_ANY_SEGWIT, false);
        }

        if (EnableDualFund)
        {
            features.SetFeature(Feature.OPTION_DUAL_FUND, false);
        }

        if (EnableOnionMessages)
        {
            features.SetFeature(Feature.OPTION_ONION_MESSAGES, false);
        }

        if (EnableChannelType)
        {
            features.SetFeature(Feature.OPTION_CHANNEL_TYPE, false);
        }

        if (EnableScidAlias)
        {
            features.SetFeature(Feature.OPTION_SCID_ALIAS, false);
        }

        if (EnablePaymentMetadata)
        {
            features.SetFeature(Feature.OPTION_PAYMENT_METADATA, false);
        }

        if (EnableZeroConf)
        {
            features.SetFeature(Feature.OPTION_ZEROCONF, false);
        }

        return features;
    }

    /// <summary>
    /// Get the Init extension for the node.
    /// </summary>
    /// <returns>The Init extension for the node.</returns>
    /// <remarks>
    /// If there are no ChainHashes, Mainnet is used as default.
    /// </remarks>
    internal TlvStream GetInitExtension()
    {
        var extension = new TlvStream();

        // If there are no ChainHashes, use Mainnet as default
        if (!ChainHashes.Any())
        {
            ChainHashes = [ChainConstants.MAIN];
        }

        var networks = new NetworksTlv(ChainHashes);
        extension.Add(networks);

        // TODO: Review this when implementing BOLT7
        // // If RemoteAddress is set, add it to the extension
        // if (RemoteAddress != null)
        // {
        //     extension.Add(new(new BigSize(3), RemoteAddress.GetAddressBytes()));
        // }

        return extension;
    }

    /// <summary>
    /// Get the node options from the features and extension.
    /// </summary>
    /// <param name="features">The features of the node.</param>
    /// <param name="extension">The extension of the node.</param>
    /// <returns>The node options.</returns>
    internal static NodeOptions GetNodeOptions(Features features, TlvStream? extension)
    {
        var options = new NodeOptions
        {
            EnableDataLossProtect = features.HasFeature(Feature.OPTION_DATA_LOSS_PROTECT),
            EnableInitialRoutingSync = features.HasFeature(Feature.INITIAL_ROUTING_SYNC),
            EnableUpfrontShutdownScript = features.HasFeature(Feature.OPTION_UPFRONT_SHUTDOWN_SCRIPT),
            EnableGossipQueries = features.HasFeature(Feature.GOSSIP_QUERIES),
            EnableExpandedGossipQueries = features.HasFeature(Feature.GOSSIP_QUERIES_EX),
            EnableStaticRemoteKey = features.HasFeature(Feature.OPTION_STATIC_REMOTE_KEY),
            EnablePaymentSecret = features.HasFeature(Feature.PAYMENT_SECRET),
            EnableBasicMpp = features.HasFeature(Feature.BASIC_MPP),
            EnableLargeChannels = features.HasFeature(Feature.OPTION_SUPPORT_LARGE_CHANNEL),
            EnableAnchorOutputs = features.HasFeature(Feature.OPTION_ANCHOR_OUTPUTS),
            EnableZeroFeeAnchorTx = features.HasFeature(Feature.OPTION_ANCHORS_ZERO_FEE_HTLC_TX),
            EnableRouteBlinding = features.HasFeature(Feature.OPTION_ROUTE_BLINDING),
            EnableBeyondSegwitShutdown = features.HasFeature(Feature.OPTION_SHUTDOWN_ANY_SEGWIT),
            EnableDualFund = features.HasFeature(Feature.OPTION_DUAL_FUND),
            EnableOnionMessages = features.HasFeature(Feature.OPTION_ONION_MESSAGES),
            EnableChannelType = features.HasFeature(Feature.OPTION_CHANNEL_TYPE),
            EnableScidAlias = features.HasFeature(Feature.OPTION_SCID_ALIAS),
            EnablePaymentMetadata = features.HasFeature(Feature.OPTION_PAYMENT_METADATA),
            EnableZeroConf = features.HasFeature(Feature.OPTION_ZEROCONF)
        };

        if (extension?.TryGetTlv(new BigSize(1), out var chainHashes) ?? false)
        {
            options.ChainHashes = Enumerable.Range(0, chainHashes!.Value.Length / ChainHash.LENGTH)
                                            .Select(i => new ChainHash(chainHashes.Value.Skip(i * 32).Take(32).ToArray()));
        }

        // TODO: Add network when implementing BOLT7

        return options;
    }
}