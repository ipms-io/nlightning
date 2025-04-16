using System.Net;

namespace NLightning.Common.Options;

using Constants;
using Enums;
using Node;
using TLVs;
using Types;

public class NodeOptions
{
    /// <summary>
    /// Enable data loss protection.
    /// </summary>
    public FeatureSupport DataLossProtect { get; set; } = FeatureSupport.COMPULSORY;

    /// <summary>
    /// Enable initial routing sync.
    /// </summary>
    public FeatureSupport InitialRoutingSync { get; set; } = FeatureSupport.NO;

    /// <summary>
    /// Enable upfront shutdown script.
    /// </summary>
    public FeatureSupport UpfrontShutdownScript { get; set; } = FeatureSupport.OPTIONAL;

    /// <summary>
    /// Enable gossip queries.
    /// </summary>
    public FeatureSupport GossipQueries { get; set; } = FeatureSupport.OPTIONAL;

    /// <summary>
    /// Enable expanded gossip queries.
    /// </summary>
    public FeatureSupport ExpandedGossipQueries { get; set; } = FeatureSupport.OPTIONAL;

    /// <summary>
    /// Enable static remote key.
    /// </summary>
    public FeatureSupport StaticRemoteKey { get; set; } = FeatureSupport.COMPULSORY;

    /// <summary>
    /// Enable payment secret.
    /// </summary>
    public FeatureSupport PaymentSecret { get; set; } = FeatureSupport.COMPULSORY;

    /// <summary>
    /// Enable basic MPP.
    /// </summary>
    public FeatureSupport BasicMpp { get; set; } = FeatureSupport.OPTIONAL;

    /// <summary>
    /// Enable large channels.
    /// </summary>
    public FeatureSupport LargeChannels { get; set; } = FeatureSupport.OPTIONAL;

    /// <summary>
    /// Enable anchor outputs.
    /// </summary>
    public FeatureSupport AnchorOutputs { get; set; } = FeatureSupport.OPTIONAL;

    /// <summary>
    /// Enable zero fee anchor tx.
    /// </summary>
    public FeatureSupport ZeroFeeAnchorTx { get; set; } = FeatureSupport.NO;

    /// <summary>
    /// Enable route blinding.
    /// </summary>
    public FeatureSupport RouteBlinding { get; set; } = FeatureSupport.OPTIONAL;

    /// <summary>
    /// Enable beyond segwit shutdown.
    /// </summary>
    public FeatureSupport BeyondSegwitShutdown { get; set; } = FeatureSupport.NO;

    /// <summary>
    /// Enable dual fund.
    /// </summary>
    public FeatureSupport DualFund { get; set; } = FeatureSupport.OPTIONAL;

    /// <summary>
    /// Enable onion messages.
    /// </summary>
    public FeatureSupport OnionMessages { get; set; } = FeatureSupport.NO;

    /// <summary>
    /// Enable channel type.
    /// </summary>
    public FeatureSupport ChannelType { get; set; } = FeatureSupport.OPTIONAL;

    /// <summary>
    /// Enable scid alias.
    /// </summary>
    public FeatureSupport ScidAlias { get; set; } = FeatureSupport.OPTIONAL;

    /// <summary>
    /// Enable payment metadata.
    /// </summary>
    public FeatureSupport PaymentMetadata { get; set; } = FeatureSupport.NO;

    /// <summary>
    /// Enable zero conf.
    /// </summary>
    public FeatureSupport ZeroConf { get; set; } = FeatureSupport.NO;

    /// <summary>
    /// The chain hashes of the node.
    /// </summary>
    /// <remarks>
    /// Initialized as Mainnet if not set.
    /// </remarks>
    public IEnumerable<ChainHash> ChainHashes { get; set; } = [];

    /// <summary>
    /// The remote address of the node.
    /// </summary>
    /// <remarks>
    /// This is used to connect to our node.
    /// </remarks>
    public IPAddress? RemoteAddress { get; set; } = null;

    /// <summary>
    /// Get Features set for the node.
    /// </summary>
    /// <returns>The features set for the node.</returns>
    /// <remarks>
    /// All features set as OPTIONAL.
    /// </remarks>
    internal FeatureSet GetNodeFeatures()
    {
        var features = new FeatureSet();

        // Set default features
        if (DataLossProtect != FeatureSupport.NO)
        {
            features.SetFeature(Feature.OPTION_DATA_LOSS_PROTECT, DataLossProtect == FeatureSupport.COMPULSORY);
        }

        if (InitialRoutingSync != FeatureSupport.NO)
        {
            features.SetFeature(Feature.INITIAL_ROUTING_SYNC, InitialRoutingSync == FeatureSupport.COMPULSORY);
        }

        if (UpfrontShutdownScript != FeatureSupport.NO)
        {
            features.SetFeature(Feature.OPTION_UPFRONT_SHUTDOWN_SCRIPT,
                                UpfrontShutdownScript == FeatureSupport.COMPULSORY);
        }

        if (GossipQueries != FeatureSupport.NO)
        {
            features.SetFeature(Feature.GOSSIP_QUERIES, GossipQueries == FeatureSupport.COMPULSORY);
        }

        if (ExpandedGossipQueries != FeatureSupport.NO)
        {
            features.SetFeature(Feature.GOSSIP_QUERIES_EX, ExpandedGossipQueries == FeatureSupport.COMPULSORY);
        }

        if (StaticRemoteKey != FeatureSupport.NO)
        {
            features.SetFeature(Feature.OPTION_STATIC_REMOTE_KEY, StaticRemoteKey == FeatureSupport.COMPULSORY);
        }

        if (PaymentSecret != FeatureSupport.NO)
        {
            features.SetFeature(Feature.PAYMENT_SECRET, PaymentSecret == FeatureSupport.COMPULSORY);
        }

        if (BasicMpp != FeatureSupport.NO)
        {
            features.SetFeature(Feature.BASIC_MPP, BasicMpp == FeatureSupport.COMPULSORY);
        }

        if (LargeChannels != FeatureSupport.NO)
        {
            features.SetFeature(Feature.OPTION_SUPPORT_LARGE_CHANNEL, LargeChannels == FeatureSupport.COMPULSORY);
        }

        if (AnchorOutputs != FeatureSupport.NO)
        {
            features.SetFeature(Feature.OPTION_ANCHOR_OUTPUTS, AnchorOutputs == FeatureSupport.COMPULSORY);
        }

        if (ZeroFeeAnchorTx != FeatureSupport.NO)
        {
            features.SetFeature(Feature.OPTION_ANCHORS_ZERO_FEE_HTLC_TX, ZeroFeeAnchorTx == FeatureSupport.COMPULSORY);
        }

        if (RouteBlinding != FeatureSupport.NO)
        {
            features.SetFeature(Feature.OPTION_ROUTE_BLINDING, RouteBlinding == FeatureSupport.COMPULSORY);
        }

        if (BeyondSegwitShutdown != FeatureSupport.NO)
        {
            features.SetFeature(Feature.OPTION_SHUTDOWN_ANY_SEGWIT, BeyondSegwitShutdown == FeatureSupport.COMPULSORY);
        }

        if (DualFund != FeatureSupport.NO)
        {
            features.SetFeature(Feature.OPTION_DUAL_FUND, DualFund == FeatureSupport.COMPULSORY);
        }

        if (OnionMessages != FeatureSupport.NO)
        {
            features.SetFeature(Feature.OPTION_ONION_MESSAGES, OnionMessages == FeatureSupport.COMPULSORY);
        }

        if (ChannelType != FeatureSupport.NO)
        {
            features.SetFeature(Feature.OPTION_CHANNEL_TYPE, ChannelType == FeatureSupport.COMPULSORY);
        }

        if (ScidAlias != FeatureSupport.NO)
        {
            features.SetFeature(Feature.OPTION_SCID_ALIAS, ScidAlias == FeatureSupport.COMPULSORY);
        }

        if (PaymentMetadata != FeatureSupport.NO)
        {
            features.SetFeature(Feature.OPTION_PAYMENT_METADATA, PaymentMetadata == FeatureSupport.COMPULSORY);
        }

        if (ZeroConf != FeatureSupport.NO)
        {
            features.SetFeature(Feature.OPTION_ZEROCONF, ZeroConf == FeatureSupport.COMPULSORY);
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
    internal NetworksTlv GetInitTlvs()
    {
        // If there are no ChainHashes, use Mainnet as default
        if (!ChainHashes.Any())
        {
            ChainHashes = [ChainConstants.MAIN];
        }

        return new NetworksTlv(ChainHashes);

        // TODO: Review this when implementing BOLT7
        // // If RemoteAddress is set, add it to the extension
        // if (RemoteAddress != null)
        // {
        //     extension.Add(new(new BigSize(3), RemoteAddress.GetAddressBytes()));
        // }
    }

    /// <summary>
    /// Get the node options from the features and extension.
    /// </summary>
    /// <param name="featureSet">The features of the node.</param>
    /// <param name="extension">The extension of the node.</param>
    /// <returns>The node options.</returns>
    internal static NodeOptions GetNodeOptions(FeatureSet featureSet, TlvStream? extension)
    {
        var options = new NodeOptions
        {
            DataLossProtect = featureSet.IsFeatureSet(Feature.OPTION_DATA_LOSS_PROTECT, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.OPTION_DATA_LOSS_PROTECT, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            InitialRoutingSync = featureSet.IsFeatureSet(Feature.INITIAL_ROUTING_SYNC, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.INITIAL_ROUTING_SYNC, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            UpfrontShutdownScript = featureSet.IsFeatureSet(Feature.OPTION_UPFRONT_SHUTDOWN_SCRIPT, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.OPTION_UPFRONT_SHUTDOWN_SCRIPT, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            GossipQueries = featureSet.IsFeatureSet(Feature.GOSSIP_QUERIES, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.GOSSIP_QUERIES, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            ExpandedGossipQueries = featureSet.IsFeatureSet(Feature.GOSSIP_QUERIES_EX, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.GOSSIP_QUERIES_EX, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            StaticRemoteKey = featureSet.IsFeatureSet(Feature.OPTION_STATIC_REMOTE_KEY, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.OPTION_STATIC_REMOTE_KEY, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            PaymentSecret = featureSet.IsFeatureSet(Feature.PAYMENT_SECRET, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.PAYMENT_SECRET, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            BasicMpp = featureSet.IsFeatureSet(Feature.BASIC_MPP, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.BASIC_MPP, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            LargeChannels = featureSet.IsFeatureSet(Feature.OPTION_SUPPORT_LARGE_CHANNEL, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.OPTION_SUPPORT_LARGE_CHANNEL, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            AnchorOutputs = featureSet.IsFeatureSet(Feature.OPTION_ANCHOR_OUTPUTS, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.OPTION_ANCHOR_OUTPUTS, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            ZeroFeeAnchorTx = featureSet.IsFeatureSet(Feature.OPTION_ANCHORS_ZERO_FEE_HTLC_TX, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.OPTION_ANCHORS_ZERO_FEE_HTLC_TX, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            RouteBlinding = featureSet.IsFeatureSet(Feature.OPTION_ROUTE_BLINDING, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.OPTION_ROUTE_BLINDING, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            BeyondSegwitShutdown = featureSet.IsFeatureSet(Feature.OPTION_SHUTDOWN_ANY_SEGWIT, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.OPTION_SHUTDOWN_ANY_SEGWIT, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            DualFund = featureSet.IsFeatureSet(Feature.OPTION_DUAL_FUND, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.OPTION_DUAL_FUND, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            OnionMessages = featureSet.IsFeatureSet(Feature.OPTION_ONION_MESSAGES, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.OPTION_ONION_MESSAGES, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            ChannelType = featureSet.IsFeatureSet(Feature.OPTION_CHANNEL_TYPE, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.OPTION_CHANNEL_TYPE, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            ScidAlias = featureSet.IsFeatureSet(Feature.OPTION_SCID_ALIAS, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.OPTION_SCID_ALIAS, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            PaymentMetadata = featureSet.IsFeatureSet(Feature.OPTION_PAYMENT_METADATA, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.OPTION_PAYMENT_METADATA, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO,
            ZeroConf = featureSet.IsFeatureSet(Feature.OPTION_ZEROCONF, true)
                ? FeatureSupport.COMPULSORY
                : featureSet.IsFeatureSet(Feature.OPTION_ZEROCONF, false)
                    ? FeatureSupport.OPTIONAL
                    : FeatureSupport.NO
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