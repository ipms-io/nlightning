using System.Diagnostics.CodeAnalysis;
using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Outputs;

using Crypto.Hashes;
using Domain.Crypto.Constants;
using Domain.Money;
using Exceptions;
using Infrastructure.Crypto.Hashes;

/// <summary>
/// Represents an offered HTLC output in a commitment transaction.
/// </summary>
public class OfferedHtlcOutput : BaseHtlcOutput
{
    public override ScriptType ScriptType => ScriptType.P2WPKH;

    [SetsRequiredMembers]
    public OfferedHtlcOutput(LightningMoney amount, ulong cltvExpiry, bool hasAnchor, PubKey localHtlcPubKey,
                             ReadOnlyMemory<byte> paymentHash, PubKey remoteHtlcPubKey, PubKey revocationPubKey)
        : base(amount,
               GenerateToRemoteHtlcScript(hasAnchor, localHtlcPubKey, paymentHash, remoteHtlcPubKey, revocationPubKey))

    {
        RevocationPubKey = revocationPubKey;
        RemoteHtlcPubKey = remoteHtlcPubKey;
        LocalHtlcPubKey = localHtlcPubKey;
        PaymentHash = paymentHash;
        CltvExpiry = cltvExpiry;
    }

    private static Script GenerateToRemoteHtlcScript(bool hasAnchor, PubKey localHtlcPubKey,
                                                     ReadOnlyMemory<byte> paymentHash, PubKey remoteHtlcPubKey,
                                                     PubKey revocationPubKey)
    {
        // Hash the revocationPubKey
        using var sha256 = new Sha256();
        Span<byte> revocationPubKeySha256Hash = stackalloc byte[CryptoConstants.Sha256HashLen];
        sha256.AppendData(revocationPubKey.ToBytes());
        sha256.GetHashAndReset(revocationPubKeySha256Hash);
        var revocationPubKeyHashRipemd160 = Ripemd160.Hash(revocationPubKeySha256Hash);

        // Hash the paymentHash
        var paymentHashRipemd160 = Ripemd160.Hash(paymentHash.Span);

        List<Op> ops =
        [
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

        if (hasAnchor)
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

        // Check if the script is correct
        if (script.IsUnspendable || !script.IsValid)
        {
            throw new InvalidScriptException("ScriptPubKey is either 'invalid' or 'unspendable'.");
        }

        return script;
    }
}