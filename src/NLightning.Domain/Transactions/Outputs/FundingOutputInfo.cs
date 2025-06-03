using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Money;
using NLightning.Domain.Transactions.Enums;
using NLightning.Domain.Transactions.Interfaces;

namespace NLightning.Domain.Transactions.Outputs;

public class FundingOutputInfo : IOutputInfo
{
    public LightningMoney Amount { get; }
    public CompactPubKey LocalFundingPubKey { get; set; }
    public CompactPubKey RemoteFundingPubKey { get; set; }
    
    public OutputType OutputType => OutputType.LocalAnchor;
    public TxId? TxId { get; set; }
    public uint? Index { get; set; }

    public FundingOutputInfo(LightningMoney amount, CompactPubKey localFundingPubKey, CompactPubKey remoteFundingPubKey)
    {
        Amount = amount;
        LocalFundingPubKey = localFundingPubKey;
        RemoteFundingPubKey = remoteFundingPubKey;
    }
}