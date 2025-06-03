using System.Diagnostics.CodeAnalysis;
using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Outputs;

using Domain.Money;

public abstract class BaseHtlcOutput : BaseOutput
{
    public required PubKey RevocationPubKey { get; init; }
    public required PubKey RemoteHtlcPubKey { get; init; }
    public required PubKey LocalHtlcPubKey { get; init; }
    public ReadOnlyMemory<byte> PaymentHash { get; set; }
    public required ulong CltvExpiry { get; init; }

    protected BaseHtlcOutput(LightningMoney amount, Script redeemScript) : base(amount, redeemScript)
    { }

    [SetsRequiredMembers]
    protected BaseHtlcOutput(LightningMoney amount, ulong cltvExpiry, PubKey localHtlcPubKey,
                             ReadOnlyMemory<byte> paymentHash, Script redeemScript, PubKey remoteHtlcPubKey,
                             PubKey revocationPubKey)
        : base(amount, redeemScript)
    {
        RevocationPubKey = revocationPubKey;
        RemoteHtlcPubKey = remoteHtlcPubKey;
        LocalHtlcPubKey = localHtlcPubKey;
        PaymentHash = paymentHash;
        CltvExpiry = cltvExpiry;
    }
}