using Microsoft.Extensions.Logging;
using NBitcoin;
using NLightning.Domain.Bitcoin.Interfaces;
using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Node.Options;
using NLightning.Infrastructure.Bitcoin.Builders;
using NLightning.Infrastructure.Bitcoin.Signers;
using NLightning.Tests.Utils.Vectors;
using CompactSignature = NLightning.Domain.Crypto.ValueObjects.CompactSignature;

namespace NLightning.Integration.Tests.BOLT3.Mocks;

public class Bolt3TestLightningSigner : LocalLightningSigner, ILightningSigner
{
    public Bolt3TestLightningSigner(NodeOptions nodeOptions, ILogger<LocalLightningSigner> logger)
        : base(new FundingOutputBuilder(), null, logger, nodeOptions, null)
    {
    }

    public new ChannelBasepoints GetChannelBasepoints(uint channelKeyIndex)
    {
        return new ChannelBasepoints();
    }

    public new ChannelBasepoints GetChannelBasepoints(ChannelId channelId)
    {
        return new ChannelBasepoints();
    }

    public new void RegisterChannel(ChannelId channelId, ChannelSigningInfo signingInfo)
    {
        base.RegisterChannel(channelId, signingInfo);
    }

    public new void ValidateSignature(ChannelId channelId, CompactSignature signature,
                                      SignedTransaction unsignedTransaction)
    {
        base.ValidateSignature(channelId, signature, unsignedTransaction);
    }

    protected override Key GenerateFundingPrivateKey(uint channelKeyIndex)
    {
        return Bolt3AppendixCVectors.NodeAFundingPrivkey;
    }
}