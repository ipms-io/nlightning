namespace NLightning.Infrastructure.Bitcoin.Builders;

using Domain.Transactions.Outputs;
using Outputs;

public interface IFundingOutputBuilder
{
    FundingOutput Build(FundingOutputInfo fundingOutputInfo);
}