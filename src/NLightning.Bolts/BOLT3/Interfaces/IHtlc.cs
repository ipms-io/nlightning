using NBitcoin;

namespace NLightning.Bolts.BOLT3.Interfaces;

public interface IHtlc
{
    Script ScriptPubKey { get; }
    Money Amount { get; }
}