using NBitcoin;

namespace NLightning.Bolts.BOLT3.Htlcs;

using Bolts.Constants;
using Common.Crypto.Hashes;
using Exceptions;
using Interfaces;

public class OfferedHtlcOutputs : IHtlc
{
    public Script ScriptPubKey { get; }
    public Money Amount { get; }

    public OfferedHtlcOutputs(PubKey revocationPubKey, PubKey remoteHtlcPubKey, PubKey localHtlcPubKey, uint256 paymentHash, Money amount, bool optionAnchors)
    {
        using var sha256 = new Sha256();

        // Hash the revocationPubKey
        sha256.AppendData(revocationPubKey.ToBytes());
        var revocationPubKeyHash = new byte[HashConstants.SHA256_HASH_LEN];
        sha256.GetHashAndReset(revocationPubKeyHash);
        revocationPubKeyHash = Ripemd160.Hash(revocationPubKeyHash);

        // Hash the paymentHash
        var paymentHashRipemd160 = Ripemd160.Hash(paymentHash.ToBytes());

        List<Op> ops = [
            OpcodeType.OP_DUP,
            OpcodeType.OP_HASH160,
            Op.GetPushOp(revocationPubKeyHash[..HashConstants.RIPEMD160_HASH_LEN]), // Get only the first 20 bytes
            OpcodeType.OP_EQUAL,
            OpcodeType.OP_IF,
            OpcodeType.OP_CHECKSIG,
            OpcodeType.OP_ELSE,
            Op.GetPushOp(remoteHtlcPubKey.ToBytes()),
            OpcodeType.OP_SWAP,
            OpcodeType.OP_SIZE,
            Op.GetPushOp(32),
            OpcodeType.OP_EQUAL,
            OpcodeType.OP_NOTIF,
            OpcodeType.OP_DROP,
            OpcodeType.OP_2,
            OpcodeType.OP_SWAP,
            Op.GetPushOp(localHtlcPubKey.ToBytes()),
            OpcodeType.OP_2,
            OpcodeType.OP_CHECKMULTISIG,
            OpcodeType.OP_ELSE,
            OpcodeType.OP_HASH160,
            Op.GetPushOp(paymentHashRipemd160),
            OpcodeType.OP_EQUALVERIFY,
            OpcodeType.OP_CHECKSIG,
            OpcodeType.OP_ENDIF
        ];

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

        var script = new Script(ops);

        // Check if script is correct
        if (script.IsUnspendable || !script.IsValid)
        {
            throw new InvalidScriptException("ScriptPubKey is either 'invalid' or 'unspendable'.");
        }

        ScriptPubKey = PayToWitScriptHashTemplate.Instance.GenerateScriptPubKey(script.WitHash);
        Amount = amount;
    }
}