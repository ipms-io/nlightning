using NBitcoin;
using NLightning.Domain.Bitcoin.Transactions.Outputs;

namespace NLightning.Infrastructure.Bitcoin.Builders;

using Domain.Bitcoin.ValueObjects;
using Outputs;

public class FundingOutputBuilder : IFundingOutputBuilder
{
    public FundingOutput Build(FundingOutputInfo fundingOutputInfo)
    {
        ArgumentNullException.ThrowIfNull(fundingOutputInfo);

        var localFundingPubKey = new PubKey(fundingOutputInfo.LocalFundingPubKey);
        var remoteFundingPubKey = new PubKey(fundingOutputInfo.RemoteFundingPubKey);
        return new FundingOutput(fundingOutputInfo.Amount, localFundingPubKey, remoteFundingPubKey)
        {
            TransactionId = fundingOutputInfo.TransactionId ?? TxId.Zero,
            Index = fundingOutputInfo.Index ?? 0
        };
    }
}