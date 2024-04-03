using System.Net;

namespace NLightning.Bolts;

using BOLT8.Dhs;
using BOLT8.Primitives;
using BOLT9;
using Common.Constants;
using Common.TLVs;

/// <summary>
/// Represents the options for a node.
/// </summary>
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
    public bool EnableDataLossProtect = false;

    /// <summary>
    /// Enable initial routing sync.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableInitialRoutingSync = false;

    /// <summary>
    /// Enable upfront shutdown script.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableUpfrontShutdownScript = false;

    /// <summary>
    /// Enable gossip queries.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableGossipQueries = false;

    /// <summary>
    /// Enable expanded gossip queries.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableExpandedGossipQueries = false;

    /// <summary>
    /// Enable static remote key.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableStaticRemoteKey = false;

    /// <summary>
    /// Enable payment secret.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnablePaymentSecret = false;

    /// <summary>
    /// Enable basic MPP.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableBasicMPP = false;

    /// <summary>
    /// Enable large channels.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableLargeChannels = false;

    /// <summary>
    /// Enable anchor outputs.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableAnchorOutputs = false;

    /// <summary>
    /// Enable zero fee anchor tx.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableZeroFeeAnchorTx = false;

    /// <summary>
    /// Enable route blinding.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableRouteBlinding = false;

    /// <summary>
    /// Enable beyond segwit shutdown.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableBeyondSegwitShutdown = false;

    /// <summary>
    /// Enable dual fund.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableDualFund = false;

    /// <summary>
    /// Enable onion messages.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableOnionMessages = false;

    /// <summary>
    /// Enable channel type.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableChannelType = false;

    /// <summary>
    /// Enable scid alias.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableScidAlias = false;

    /// <summary>
    /// Enable payment metadata.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnablePaymentMetadata = false;

    /// <summary>
    /// Enable zero conf.
    /// </summary>
    /// <remarks>
    /// This will be added to the features of the node as OPTIONAL.
    /// </remarks>
    public bool EnableZeroConf = false;
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
        KeyPair = new Secp256k1().GenerateKeyPair();
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
            features.SetFeature(Feature.OptionDataLossProtect, false);
        }

        if (EnableInitialRoutingSync)
        {
            features.SetFeature(Feature.InitialRoutingSync, false);
        }

        if (EnableUpfrontShutdownScript)
        {
            features.SetFeature(Feature.OptionUpfrontShutdownScript, false);
        }

        if (EnableGossipQueries)
        {
            features.SetFeature(Feature.GossipQueries, false);
        }

        if (EnableExpandedGossipQueries)
        {
            features.SetFeature(Feature.GossipQueriesEx, false);
        }

        if (EnableStaticRemoteKey)
        {
            features.SetFeature(Feature.OptionStaticRemoteKey, false);
        }

        if (EnablePaymentSecret)
        {
            features.SetFeature(Feature.PaymentSecret, false);
        }

        if (EnableBasicMPP)
        {
            features.SetFeature(Feature.BasicMpp, false);
        }

        if (EnableLargeChannels)
        {
            features.SetFeature(Feature.OptionSupportLargeChannel, false);
        }

        if (EnableAnchorOutputs)
        {
            features.SetFeature(Feature.OptionAnchorOutputs, false);
        }

        if (EnableZeroFeeAnchorTx)
        {
            features.SetFeature(Feature.OptionAnchorsZeroFeeHtlcTx, false);
        }

        if (EnableRouteBlinding)
        {
            features.SetFeature(Feature.OptionRouteBlinding, false);
        }

        if (EnableBeyondSegwitShutdown)
        {
            features.SetFeature(Feature.OptionShutdownAnySegwit, false);
        }

        if (EnableDualFund)
        {
            features.SetFeature(Feature.OptionDualFund, false);
        }

        if (EnableOnionMessages)
        {
            features.SetFeature(Feature.OptionOnionMessages, false);
        }

        if (EnableChannelType)
        {
            features.SetFeature(Feature.OptionChannelType, false);
        }

        if (EnableScidAlias)
        {
            features.SetFeature(Feature.OptionScidAlias, false);
        }

        if (EnablePaymentMetadata)
        {
            features.SetFeature(Feature.OptionPaymentMetadata, false);
        }

        if (EnableZeroConf)
        {
            features.SetFeature(Feature.OptionZeroconf, false);
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
    internal TLVStream GetInitExtension()
    {
        var extension = new TLVStream();

        // If there are no ChainHashes, use Mainnet as default
        if (!ChainHashes.Any())
        {
            ChainHashes = [ChainConstants.Main];
        }

        var networks = new NetworksTLV(ChainHashes);
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
    /// <remarks>
    internal static NodeOptions GetNodeOptions(Features features, TLVStream? extension)
    {
        var options = new NodeOptions
        {
            EnableDataLossProtect = features.HasFeature(Feature.OptionDataLossProtect),
            EnableInitialRoutingSync = features.HasFeature(Feature.InitialRoutingSync),
            EnableUpfrontShutdownScript = features.HasFeature(Feature.OptionUpfrontShutdownScript),
            EnableGossipQueries = features.HasFeature(Feature.GossipQueries),
            EnableExpandedGossipQueries = features.HasFeature(Feature.GossipQueriesEx),
            EnableStaticRemoteKey = features.HasFeature(Feature.OptionStaticRemoteKey),
            EnablePaymentSecret = features.HasFeature(Feature.PaymentSecret),
            EnableBasicMPP = features.HasFeature(Feature.BasicMpp),
            EnableLargeChannels = features.HasFeature(Feature.OptionSupportLargeChannel),
            EnableAnchorOutputs = features.HasFeature(Feature.OptionAnchorOutputs),
            EnableZeroFeeAnchorTx = features.HasFeature(Feature.OptionAnchorsZeroFeeHtlcTx),
            EnableRouteBlinding = features.HasFeature(Feature.OptionRouteBlinding),
            EnableBeyondSegwitShutdown = features.HasFeature(Feature.OptionShutdownAnySegwit),
            EnableDualFund = features.HasFeature(Feature.OptionDualFund),
            EnableOnionMessages = features.HasFeature(Feature.OptionOnionMessages),
            EnableChannelType = features.HasFeature(Feature.OptionChannelType),
            EnableScidAlias = features.HasFeature(Feature.OptionScidAlias),
            EnablePaymentMetadata = features.HasFeature(Feature.OptionPaymentMetadata),
            EnableZeroConf = features.HasFeature(Feature.OptionZeroconf)
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