using NBitcoin;

namespace NLightning.Bolts.BOLT3.Htlcs;

using BOLT3.Hashes;
using BOLT8.Hashes;
using Bolts.Constants;
using Exceptions;

public class ReceivedHtlcOutputs
{
    public Script ScriptPubKey { get; }
    public Money Amount { get; }

    public ReceivedHtlcOutputs(PubKey revocationPubKey, PubKey remoteHtlcPubKey, PubKey localHtlcPubKey, uint cltvExpiry, uint256 paymentHash, Money amount, bool optionAnchors)
    {
        using var sha256 = new SHA256();
        using var ripemd160 = new RIPEMD160();

        // Hash the revocationPubKey
        sha256.AppendData(revocationPubKey.ToBytes());
        var revocationPubKeyHash = new byte[HashConstants.SHA256_HASH_LEN];
        sha256.GetHashAndReset(revocationPubKeyHash);
        ripemd160.AppendData(revocationPubKeyHash);
        ripemd160.GetHashAndReset(revocationPubKeyHash);

        // Hash the paymentHash
        var paymentHashRipemd160 = new byte[HashConstants.RIPEMD160_HASH_LEN];
        ripemd160.AppendData(paymentHash.ToBytes());
        ripemd160.GetHashAndReset(paymentHashRipemd160);

        List<Op> ops = [
            OpcodeType.OP_DUP,
            OpcodeType.OP_HASH160,
            Op.GetPushOp(revocationPubKeyHash),
            OpcodeType.OP_EQUAL,
            OpcodeType.OP_IF,
            OpcodeType.OP_CHECKSIG,
            OpcodeType.OP_ELSE,
            Op.GetPushOp(remoteHtlcPubKey.ToBytes()),
            OpcodeType.OP_SWAP,
            OpcodeType.OP_SIZE,
            Op.GetPushOp(32),
            OpcodeType.OP_EQUAL,
            OpcodeType.OP_IF,
            OpcodeType.OP_HASH160,
            Op.GetPushOp(paymentHashRipemd160),
            OpcodeType.OP_EQUALVERIFY,
            OpcodeType.OP_2,
            OpcodeType.OP_SWAP,
            Op.GetPushOp(localHtlcPubKey.ToBytes()),
            OpcodeType.OP_2,
            OpcodeType.OP_CHECKMULTISIG,
            OpcodeType.OP_ELSE,
            OpcodeType.OP_DROP,
            Op.GetPushOp(cltvExpiry),
            OpcodeType.OP_CHECKLOCKTIMEVERIFY,
            OpcodeType.OP_DROP,
            OpcodeType.OP_CHECKSIG,
            OpcodeType.OP_ENDIF,
            OpcodeType.OP_ENDIF
        ];

        Script script;
        if (optionAnchors)
        {
            ops.AddRange([
                OpcodeType.OP_1,
                OpcodeType.OP_CHECKSEQUENCEVERIFY,
                OpcodeType.OP_DROP
            ]);
        }

        // Close last IF
        ops.Add(OpcodeType.OP_ENDIF);

        script = new Script(ops);

        // Check if script is correct
        if (script.IsUnspendable || !script.IsValid)
        {
            throw new InvalidScriptException("Script is either 'invalid' or 'unspendable'.");
        }

        ScriptPubKey = PayToWitScriptHashTemplate.Instance.GenerateScriptPubKey(script.WitHash);
        Amount = amount;
    }
}