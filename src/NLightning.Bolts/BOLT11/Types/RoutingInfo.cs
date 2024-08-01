using NBitcoin;

namespace NLightning.Bolts.BOLT11.Types;

public sealed class RoutingInfo(PubKey pubKey, ShortChannelId shortChannelId, int feeBaseMsat, int feeProportionalMillionths, short cltvExpiryDelta)
{
    public PubKey PubKey { get; set; } = pubKey;
    public ShortChannelId ShortChannelId { get; set; } = shortChannelId;
    public int FeeBaseMsat { get; set; } = feeBaseMsat;
    public int FeeProportionalMillionths { get; set; } = feeProportionalMillionths;
    public short CltvExpiryDelta { get; set; } = cltvExpiryDelta;
}