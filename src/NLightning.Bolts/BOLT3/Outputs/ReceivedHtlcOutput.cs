using System.Diagnostics.CodeAnalysis;
using NBitcoin;

namespace NLightning.Bolts.BOLT3.Outputs;

using Common.Constants;
using Common.Crypto.Hashes;
using Common.Managers;

/// <summary>
/// Represents a received HTLC output in a commitment transaction.
/// </summary>
public class ReceivedHtlcOutput : BaseHtlcOutput
{
    public override ScriptType ScriptType => ScriptType.P2WSH;

    [SetsRequiredMembers]
    public ReceivedHtlcOutput(PubKey revocationPubKey, PubKey remoteHtlcPubKey, PubKey localHtlcPubKey,
                              ReadOnlyMemory<byte> paymentHash, LightningMoney amount, ulong? cltvExpiry = null)
        : base(GenerateToLocalHtlcScript(revocationPubKey, remoteHtlcPubKey, localHtlcPubKey, paymentHash,
                                         cltvExpiry ?? ConfigManager.Instance.DefaultCltvExpiry),
               amount)
    {
        RevocationPubKey = revocationPubKey;
        RemoteHtlcPubKey = remoteHtlcPubKey;
        LocalHtlcPubKey = localHtlcPubKey;
        PaymentHash = paymentHash;
        CltvExpiry = cltvExpiry ?? ConfigManager.Instance.DefaultCltvExpiry;
    }

    private static Script GenerateToLocalHtlcScript(PubKey revocationPubKey, PubKey remoteHtlcPubKey, PubKey localHtlcPubKey, ReadOnlyMemory<byte> paymentHash, ulong cltvExpiry)
    {
        // Hash the revocationPubKey
        using var sha256 = new Sha256();
        Span<byte> revocationPubKeySha256Hash = stackalloc byte[CryptoConstants.SHA256_HASH_LEN];
        sha256.AppendData(revocationPubKey.ToBytes());
        sha256.GetHashAndReset(revocationPubKeySha256Hash);
        var revocationPubKeyHashRipemd160 = Ripemd160.Hash(revocationPubKeySha256Hash);

        // Hash the paymentHash
        var paymentHashRipemd160 = Ripemd160.Hash(paymentHash.Span);

        List<Op> ops = [
            OpcodeType.OP_DUP,
            OpcodeType.OP_HASH160,
            Op.GetPushOp(revocationPubKeyHashRipemd160),
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
            Op.GetPushOp((long)cltvExpiry),
            OpcodeType.OP_CHECKLOCKTIMEVERIFY,
            OpcodeType.OP_DROP,
            OpcodeType.OP_CHECKSIG,
            OpcodeType.OP_ENDIF
        ];

        if (ConfigManager.Instance.IsOptionAnchorOutput)
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

        return script;
    }
}