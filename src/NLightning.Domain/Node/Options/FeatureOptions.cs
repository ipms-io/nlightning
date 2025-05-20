using System.Net;

namespace NLightning.Domain.Node.Options;

using Enums;
using Protocol.Constants;
using Protocol.Models;
using Protocol.Tlv;
using ValueObjects;

public class FeatureOptions
{
    /// <summary>
    /// Enable data loss protection.
    /// </summary>
    public FeatureSupport DataLossProtect { get; set; } = FeatureSupport.Compulsory;

    /// <summary>
    /// Enable initial routing sync.
    /// </summary>
    public FeatureSupport InitialRoutingSync { get; set; } = FeatureSupport.No;

    /// <summary>
    /// Enable upfront shutdown script.
    /// </summary>
    public FeatureSupport UpfrontShutdownScript { get; set; } = FeatureSupport.Optional;

    /// <summary>
    /// Enable gossip queries.
    /// </summary>
    public FeatureSupport GossipQueries { get; set; } = FeatureSupport.Optional;

    /// <summary>
    /// Enable expanded gossip queries.
    /// </summary>
    public FeatureSupport ExpandedGossipQueries { get; set; } = FeatureSupport.Optional;

    /// <summary>
    /// Enable static remote key.
    /// </summary>
    public FeatureSupport StaticRemoteKey { get; set; } = FeatureSupport.Compulsory;

    /// <summary>
    /// Enable payment secret.
    /// </summary>
    public FeatureSupport PaymentSecret { get; set; } = FeatureSupport.Compulsory;

    /// <summary>
    /// Enable basic MPP.
    /// </summary>
    public FeatureSupport BasicMpp { get; set; } = FeatureSupport.Optional;

    /// <summary>
    /// Enable large channels.
    /// </summary>
    public FeatureSupport LargeChannels { get; set; } = FeatureSupport.Optional;

    /// <summary>
    /// Enable anchor outputs.
    /// </summary>
    public FeatureSupport AnchorOutputs { get; set; } = FeatureSupport.Optional;

    /// <summary>
    /// Enable zero fee anchor tx.
    /// </summary>
    public FeatureSupport ZeroFeeAnchorTx { get; set; } = FeatureSupport.No;

    /// <summary>
    /// Enable route blinding.
    /// </summary>
    public FeatureSupport RouteBlinding { get; set; } = FeatureSupport.Optional;

    /// <summary>
    /// Enable beyond segwit shutdown.
    /// </summary>
    public FeatureSupport BeyondSegwitShutdown { get; set; } = FeatureSupport.No;

    /// <summary>
    /// Enable dual fund.
    /// </summary>
    public FeatureSupport DualFund { get; set; } = FeatureSupport.Optional;

    /// <summary>
    /// Enable onion messages.
    /// </summary>
    public FeatureSupport OnionMessages { get; set; } = FeatureSupport.No;

    /// <summary>
    /// Enable channel type.
    /// </summary>
    public FeatureSupport ChannelType { get; set; } = FeatureSupport.Optional;

    /// <summary>
    /// Enable scid alias.
    /// </summary>
    public FeatureSupport ScidAlias { get; set; } = FeatureSupport.Optional;

    /// <summary>
    /// Enable payment metadata.
    /// </summary>
    public FeatureSupport PaymentMetadata { get; set; } = FeatureSupport.No;

    /// <summary>
    /// Enable zero conf.
    /// </summary>
    public FeatureSupport ZeroConf { get; set; } = FeatureSupport.No;

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
    /// All features set as Optional.
    /// </remarks>
    public FeatureSet GetNodeFeatures()
    {
        var features = new FeatureSet();

        // Set default features
        if (DataLossProtect != FeatureSupport.No)
        {
            features.SetFeature(Feature.OptionDataLossProtect, DataLossProtect == FeatureSupport.Compulsory);
        }

        if (InitialRoutingSync != FeatureSupport.No)
        {
            features.SetFeature(Feature.InitialRoutingSync, InitialRoutingSync == FeatureSupport.Compulsory);
        }

        if (UpfrontShutdownScript != FeatureSupport.No)
        {
            features.SetFeature(Feature.OptionUpfrontShutdownScript,
                                UpfrontShutdownScript == FeatureSupport.Compulsory);
        }

        if (GossipQueries != FeatureSupport.No)
        {
            features.SetFeature(Feature.GossipQueries, GossipQueries == FeatureSupport.Compulsory);
        }

        if (ExpandedGossipQueries != FeatureSupport.No)
        {
            features.SetFeature(Feature.GossipQueriesEx, ExpandedGossipQueries == FeatureSupport.Compulsory);
        }

        if (StaticRemoteKey != FeatureSupport.No)
        {
            features.SetFeature(Feature.OptionStaticRemoteKey, StaticRemoteKey == FeatureSupport.Compulsory);
        }

        if (PaymentSecret != FeatureSupport.No)
        {
            features.SetFeature(Feature.PaymentSecret, PaymentSecret == FeatureSupport.Compulsory);
        }

        if (BasicMpp != FeatureSupport.No)
        {
            features.SetFeature(Feature.BasicMpp, BasicMpp == FeatureSupport.Compulsory);
        }

        if (LargeChannels != FeatureSupport.No)
        {
            features.SetFeature(Feature.OptionSupportLargeChannel, LargeChannels == FeatureSupport.Compulsory);
        }

        if (AnchorOutputs != FeatureSupport.No)
        {
            features.SetFeature(Feature.OptionAnchorOutputs, AnchorOutputs == FeatureSupport.Compulsory);
        }

        if (ZeroFeeAnchorTx != FeatureSupport.No)
        {
            features.SetFeature(Feature.OptionAnchorsZeroFeeHtlcTx, ZeroFeeAnchorTx == FeatureSupport.Compulsory);
        }

        if (RouteBlinding != FeatureSupport.No)
        {
            features.SetFeature(Feature.OptionRouteBlinding, RouteBlinding == FeatureSupport.Compulsory);
        }

        if (BeyondSegwitShutdown != FeatureSupport.No)
        {
            features.SetFeature(Feature.OptionShutdownAnySegwit, BeyondSegwitShutdown == FeatureSupport.Compulsory);
        }

        if (DualFund != FeatureSupport.No)
        {
            features.SetFeature(Feature.OptionDualFund, DualFund == FeatureSupport.Compulsory);
        }

        if (OnionMessages != FeatureSupport.No)
        {
            features.SetFeature(Feature.OptionOnionMessages, OnionMessages == FeatureSupport.Compulsory);
        }

        if (ChannelType != FeatureSupport.No)
        {
            features.SetFeature(Feature.OptionChannelType, ChannelType == FeatureSupport.Compulsory);
        }

        if (ScidAlias != FeatureSupport.No)
        {
            features.SetFeature(Feature.OptionScidAlias, ScidAlias == FeatureSupport.Compulsory);
        }

        if (PaymentMetadata != FeatureSupport.No)
        {
            features.SetFeature(Feature.OptionPaymentMetadata, PaymentMetadata == FeatureSupport.Compulsory);
        }

        if (ZeroConf != FeatureSupport.No)
        {
            features.SetFeature(Feature.OptionZeroconf, ZeroConf == FeatureSupport.Compulsory);
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
    public static FeatureOptions GetNodeOptions(FeatureSet featureSet, TlvStream? extension)
    {
        var options = new FeatureOptions
        {
            DataLossProtect = featureSet.IsFeatureSet(Feature.OptionDataLossProtect, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.OptionDataLossProtect, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            InitialRoutingSync = featureSet.IsFeatureSet(Feature.InitialRoutingSync, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.InitialRoutingSync, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            UpfrontShutdownScript = featureSet.IsFeatureSet(Feature.OptionUpfrontShutdownScript, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.OptionUpfrontShutdownScript, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            GossipQueries = featureSet.IsFeatureSet(Feature.GossipQueries, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.GossipQueries, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            ExpandedGossipQueries = featureSet.IsFeatureSet(Feature.GossipQueriesEx, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.GossipQueriesEx, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            StaticRemoteKey = featureSet.IsFeatureSet(Feature.OptionStaticRemoteKey, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.OptionStaticRemoteKey, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            PaymentSecret = featureSet.IsFeatureSet(Feature.PaymentSecret, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.PaymentSecret, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            BasicMpp = featureSet.IsFeatureSet(Feature.BasicMpp, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.BasicMpp, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            LargeChannels = featureSet.IsFeatureSet(Feature.OptionSupportLargeChannel, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.OptionSupportLargeChannel, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            AnchorOutputs = featureSet.IsFeatureSet(Feature.OptionAnchorOutputs, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.OptionAnchorOutputs, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            ZeroFeeAnchorTx = featureSet.IsFeatureSet(Feature.OptionAnchorsZeroFeeHtlcTx, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.OptionAnchorsZeroFeeHtlcTx, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            RouteBlinding = featureSet.IsFeatureSet(Feature.OptionRouteBlinding, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.OptionRouteBlinding, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            BeyondSegwitShutdown = featureSet.IsFeatureSet(Feature.OptionShutdownAnySegwit, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.OptionShutdownAnySegwit, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            DualFund = featureSet.IsFeatureSet(Feature.OptionDualFund, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.OptionDualFund, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            OnionMessages = featureSet.IsFeatureSet(Feature.OptionOnionMessages, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.OptionOnionMessages, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            ChannelType = featureSet.IsFeatureSet(Feature.OptionChannelType, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.OptionChannelType, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            ScidAlias = featureSet.IsFeatureSet(Feature.OptionScidAlias, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.OptionScidAlias, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            PaymentMetadata = featureSet.IsFeatureSet(Feature.OptionPaymentMetadata, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.OptionPaymentMetadata, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No,
            ZeroConf = featureSet.IsFeatureSet(Feature.OptionZeroconf, true)
                ? FeatureSupport.Compulsory
                : featureSet.IsFeatureSet(Feature.OptionZeroconf, false)
                    ? FeatureSupport.Optional
                    : FeatureSupport.No
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