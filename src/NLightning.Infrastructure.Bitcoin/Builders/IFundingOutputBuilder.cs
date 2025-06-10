using NLightning.Domain.Bitcoin.Transactions.Outputs;

namespace NLightning.Infrastructure.Bitcoin.Builders;

using Outputs;

public interface IFundingOutputBuilder
{
    FundingOutput Build(FundingOutputInfo fundingOutputInfo);
}