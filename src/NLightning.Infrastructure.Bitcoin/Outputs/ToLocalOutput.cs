using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Outputs;

using Domain.Money;
using Exceptions;

/// <summary>
/// Represents a to_local output in a commitment transaction.
/// </summary>
public class ToLocalOutput : BaseOutput
{
    public override ScriptType ScriptType => ScriptType.P2WSH;

    public PubKey LocalDelayedPubKey { get; }
    public PubKey RevocationPubKey { get; }
    public uint ToSelfDelay { get; }

    public ToLocalOutput(LightningMoney amount, PubKey localDelayedPubKey, PubKey revocationPubKey, uint toSelfDelay)
        : base(amount, GenerateToLocalScript(localDelayedPubKey, revocationPubKey, toSelfDelay))
    {
        ArgumentNullException.ThrowIfNull(localDelayedPubKey);
        ArgumentNullException.ThrowIfNull(revocationPubKey);

        LocalDelayedPubKey = localDelayedPubKey;
        RevocationPubKey = revocationPubKey;
        ToSelfDelay = toSelfDelay;
    }

    private static Script GenerateToLocalScript(PubKey localDelayedPubKey, PubKey revocationPubKey, uint toSelfDelay)
    {
        /* The following script can be read as:
         ** spendingPubKey = the pubkey trying to sign this spend
         ** signature = the signature given by spendingPubKey
         ** nSequence = Provided by the spending transaction
         **
         ** if (revocationPubKey = spendingPubKey)
         ** { // Revocation key path
         **   return revocationPubKey
         ** }
         ** else
         ** { // Delayed key path
         **   if (nSequence < toSelfDelay)
         **   {
         **     exit
         **   }
         **   else
         **   {
         **     return localDelayedPubKey
         **   }
         ** }
         ** if (signature is valid for spendingPubKey)
         ** {
         **   return true
         ** }
         */

        ArgumentNullException.ThrowIfNull(localDelayedPubKey);
        ArgumentNullException.ThrowIfNull(revocationPubKey);

        var script = new Script(
            OpcodeType.OP_IF,
            Op.GetPushOp(revocationPubKey.ToBytes()),
            OpcodeType.OP_ELSE,
            Op.GetPushOp(toSelfDelay),
            OpcodeType.OP_CHECKSEQUENCEVERIFY,
            OpcodeType.OP_DROP,
            Op.GetPushOp(localDelayedPubKey.ToBytes()),
            OpcodeType.OP_ENDIF,
            OpcodeType.OP_CHECKSIG
        );

        // Check if the script is correct
        if (script.IsUnspendable || !script.IsValid)
        {
            throw new InvalidScriptException("ScriptPubKey is either 'invalid' or 'unspendable'.");
        }

        return script;
    }
}