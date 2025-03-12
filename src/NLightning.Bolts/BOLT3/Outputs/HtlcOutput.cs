using NBitcoin;

namespace NLightning.Bolts.BOLT3.Outputs;

public class HtlcOutput : OutputBase
{
    public PubKey RevocationPubKey { get; }
    public PubKey LocalDelayedPubKey { get; }
    public ulong ToSelfDelay { get; }
    
    public HtlcOutput(PubKey revocationPubKey, PubKey localDelayedPubKey, ulong toSelfDelay, ulong amountSats)
        : base(GenerateHtlcOutputScript(revocationPubKey, localDelayedPubKey, toSelfDelay), amountSats)
    {
        RevocationPubKey = revocationPubKey;
        LocalDelayedPubKey = localDelayedPubKey;
        ToSelfDelay = toSelfDelay;
    }
    
    private static Script GenerateHtlcOutputScript(PubKey revocationPubKey, PubKey localDelayedPubKey, ulong toSelfDelay)
    {
        return new Script(
            OpcodeType.OP_IF,
            Op.GetPushOp(revocationPubKey.ToBytes()),
            OpcodeType.OP_ELSE,
            Op.GetPushOp((long)toSelfDelay),
            OpcodeType.OP_CHECKSEQUENCEVERIFY,
            OpcodeType.OP_DROP,
            Op.GetPushOp(localDelayedPubKey.ToBytes()),
            OpcodeType.OP_ENDIF,
            OpcodeType.OP_CHECKSIG
        );
    }
}