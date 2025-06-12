namespace NLightning.Domain.Bitcoin.Transactions.Outputs;

using Enums;
using Interfaces;
using ValueObjects;
using Crypto.ValueObjects;
using Money;

public class FundingOutputInfo : IOutputInfo
{
    public LightningMoney Amount { get; }
    public CompactPubKey LocalFundingPubKey { get; set; }
    public CompactPubKey RemoteFundingPubKey { get; set; }

    public OutputType OutputType => OutputType.Funding;
    public TxId? TransactionId { get; set; }
    public ushort? Index { get; set; }

    public FundingOutputInfo(LightningMoney amount, CompactPubKey localFundingPubKey, CompactPubKey remoteFundingPubKey)
    {
        Amount = amount;
        LocalFundingPubKey = localFundingPubKey;
        RemoteFundingPubKey = remoteFundingPubKey;
    }

    public FundingOutputInfo(LightningMoney amount, CompactPubKey localFundingPubKey, CompactPubKey remoteFundingPubKey,
                             TxId transactionId, ushort index)
        : this(amount, localFundingPubKey, remoteFundingPubKey)
    {
        TransactionId = transactionId;
        Index = index;
    }
}