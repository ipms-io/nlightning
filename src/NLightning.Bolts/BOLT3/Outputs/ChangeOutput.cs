using NBitcoin;

namespace NLightning.Bolts.BOLT3.Outputs;

public class ChangeOutput(Script scriptPubKey, ulong amountSats = 0) : OutputBase(scriptPubKey, amountSats);