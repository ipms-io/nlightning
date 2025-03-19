using NBitcoin;

namespace NLightning.Bolts.BOLT3.Outputs;

using Common.Constants;
using Common.Crypto.Hashes;
using Common.Managers;

/// <summary>
/// Represents a received HTLC output in a commitment transaction.
/// </summary>
public class ReceivedHtlcOutput : BaseOutput
{
    public override ScriptType ScriptType => ScriptType.P2WSH;

    public PubKey RevocationPubKey { get; }
    public PubKey RemoteHtlcPubKey { get; }
    public PubKey LocalHtlcPubKey { get; }
    public ReadOnlyMemory<byte> PaymentHash { get; set; }
    public ulong CltvExpiry { get; }

    public ReceivedHtlcOutput(PubKey revocationPubKey, PubKey remoteHtlcPubKey, PubKey localHtlcPubKey, ReadOnlyMemory<byte> paymentHash, LightningMoney amountSats, ulong? cltvExpiry = null)
        : base(GenerateToLocalHtlcScript(revocationPubKey, remoteHtlcPubKey, localHtlcPubKey, paymentHash, cltvExpiry ?? ConfigManager.Instance.DefaultCltvExpiry), amountSats)
    {
        RevocationPubKey = revocationPubKey;
        RemoteHtlcPubKey = remoteHtlcPubKey;
        LocalHtlcPubKey = localHtlcPubKey;
        PaymentHash = paymentHash;
        CltvExpiry = cltvExpiry ?? ConfigManager.Instance.DefaultCltvExpiry;
    }

    private static Script GenerateToLocalHtlcScript(PubKey revocationPubKey, PubKey remoteHtlcPubKey, PubKey localHtlcPubKey, ReadOnlyMemory<byte> paymentHash, ulong cltvExpiry)
    {
        using var sha256 = new Sha256();
        Span<byte> revocationPubKeySha256Hash = stackalloc byte[CryptoConstants.SHA256_HASH_LEN];
        sha256.AppendData(revocationPubKey.ToBytes());
        sha256.GetHashAndReset(revocationPubKeySha256Hash);
        var revocationPubKeyHashRipemd160 = Ripemd160.Hash(revocationPubKeySha256Hash);

        var paymentHashRipemd160 = Ripemd160.Hash(paymentHash.Span);

        var baseScript = new Script(
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
        ).ToOps().ToList();

        if (!ConfigManager.Instance.IsOptionAnchorOutput)
        {
            return new Script(baseScript.Append(OpcodeType.OP_ENDIF));
        }

        baseScript.AddRange(new Script(
            OpcodeType.OP_1,
            OpcodeType.OP_CHECKSEQUENCEVERIFY,
            OpcodeType.OP_DROP,
            OpcodeType.OP_ENDIF
        ).ToOps());
        return new Script(baseScript);
    }
}