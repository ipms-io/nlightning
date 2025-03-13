using NBitcoin;

namespace NLightning.Bolts.BOLT3.Outputs;

public class ChangeOutput(Script scriptPubKey, ulong amountSats) : OutputBase(scriptPubKey, amountSats);