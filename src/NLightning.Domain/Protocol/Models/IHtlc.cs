using NBitcoin;

namespace NLightning.Domain.Protocol.Models;

using Money;

public interface IHtlc
{
    Script ScriptPubKey { get; }
    LightningMoney Amount { get; }
}