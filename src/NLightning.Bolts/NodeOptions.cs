using System.Net;
using NLightning.Bolts.BOLT8.Dhs;
using NLightning.Bolts.BOLT8.Primitives;
using NLightning.Bolts.BOLT9;
using NLightning.Common.Constants;

namespace NLightning.Bolts;

public sealed class NodeOptions
{
    #region Crypto
    internal KeyPair KeyPair;
    #endregion

    #region Features
    public bool EnableDataLossProtect = false;
    public bool EnableInitialRoutingSync = false;
    public bool EnableUpfrontShutdownScript = false;
    public bool EnableGossipQueries = false;
    public bool EnableExpandedGossipQueries = false;
    public bool EnableStaticRemoteKey = false;
    public bool EnablePaymentSecret = false;
    public bool EnableBasicMPP = false;
    public bool EnableLargeChannels = false;
    public bool EnableAnchorOutputs = false;
    public bool EnableZeroFeeAnchorTx = false;
    public bool EnableRouteBlinding = false;
    public bool EnableBeyondSegwitShutdown = false;
    public bool EnableDualFund = false;
    public bool EnableOnionMessages = false;
    public bool EnableChannelType = false;
    public bool EnableScidAlias = false;
    public bool EnablePaymentMetadata = false;
    public bool EnableZeroConf = false;
    #endregion

    #region InitExtension
    public IEnumerable<ChainHash> ChainHashes = [];
    public IPAddress? RemoteAddress = null;
    #endregion

    public NodeOptions()
    {
        KeyPair = new Secp256k1().GenerateKeyPair();
    }
    public NodeOptions(NBitcoin.Key p)
    {
        KeyPair = new KeyPair(p);
    }

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

    internal TLVStream GetInitExtension()
    {
        var extension = new TLVStream();

        // If there are no ChainHashes, use Mainnet as default
        if (!ChainHashes.Any())
        {
            ChainHashes = [ChainConstants.Regtest];
        }

        // Concatenate all ChainHashes bytes and add it to the extension
        var chainHashes = ChainHashes.SelectMany(x => (byte[])x).ToArray();
        extension.Add(new(new BigSize(1), chainHashes));

        // TODO: Review this when implementing BOLT7
        // // If RemoteAddress is set, add it to the extension
        // if (RemoteAddress != null)
        // {
        //     extension.Add(new(new BigSize(3), RemoteAddress.GetAddressBytes()));
        // }

        return extension;
    }

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