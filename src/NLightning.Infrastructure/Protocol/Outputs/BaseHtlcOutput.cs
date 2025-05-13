using System.Diagnostics.CodeAnalysis;
using NBitcoin;
using NLightning.Domain.Money;

namespace NLightning.Infrastructure.Protocol.Outputs;

using Domain.ValueObjects;

public abstract class BaseHtlcOutput : BaseOutput
{
    public required PubKey RevocationPubKey { get; init; }
    public required PubKey RemoteHtlcPubKey { get; init; }
    public required PubKey LocalHtlcPubKey { get; init; }
    public ReadOnlyMemory<byte> PaymentHash { get; set; }
    public required ulong CltvExpiry { get; init; }

    protected BaseHtlcOutput(Script redeemScript, LightningMoney amount) : base(redeemScript, amount)
    {
    }

    [SetsRequiredMembers]
    protected BaseHtlcOutput(Script redeemScript, LightningMoney amount, PubKey revocationPubKey,
                             PubKey remoteHtlcPubKey, PubKey localHtlcPubKey, ReadOnlyMemory<byte> paymentHash,
                             ulong cltvExpiry)
        : base(redeemScript, amount)
    {
        RevocationPubKey = revocationPubKey;
        RemoteHtlcPubKey = remoteHtlcPubKey;
        LocalHtlcPubKey = localHtlcPubKey;
        PaymentHash = paymentHash;
        CltvExpiry = cltvExpiry;
    }
}