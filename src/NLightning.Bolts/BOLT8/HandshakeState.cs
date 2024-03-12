using System.Text;
using NBitcoin;
using Org.BouncyCastle.Crypto.Digests;

namespace NLightning.Bolts.BOLT8;

public class HandshakeState
{
    public Memory<byte> CK { get; set; }
    public Memory<byte> H { get; set; }
    public Memory<byte> TempK1 { get; set; }
    public Memory<byte> TempK2 { get; set; }
    public Memory<byte> TempK3 { get; set; }
    public Memory<byte> E { get; set; }
    public Memory<byte> S { get; set; }

    public static HandshakeState Initialize(PubKey pubKey)
    {
        var state = new HandshakeState();

        // Set H = SHA256("Noise_XK_secp256k1_ChaChaPoly_SHA256")
        var digest = new Sha256Digest();
        state.H = new byte[digest.GetDigestSize()];
        digest.BlockUpdate(Encoding.UTF8.GetBytes(Constants.PROTOCOL_NAME), 0, Constants.PROTOCOL_NAME.Length);
        digest.DoFinal(state.H.Span);

        // Set CK = H
        state.CK = state.H;

        // Set H = SHA256(H || prologue)
        // concatentate H and prologue
        digest = new Sha256Digest();
        var prologue = Encoding.UTF8.GetBytes(Constants.PROLOGUE);
        Span<byte> hPrologue = new byte[state.H.Length + prologue.Length];
        state.H.Span.CopyTo(hPrologue);
        prologue.CopyTo(hPrologue[state.H.Length..]);
        digest.BlockUpdate(hPrologue);
        digest.DoFinal(state.H.Span);

        // Set H = SHA256(H || pubKey.serializeCompressed)
        // concatentate H and pubKey
        digest = new Sha256Digest();
        var pubKeyBytes = pubKey.Compress().ToBytes();
        Span<byte> hPubKey = new byte[state.H.Length + pubKeyBytes.Length];
        state.H.Span.CopyTo(hPubKey);
        pubKeyBytes.CopyTo(hPubKey[state.H.Length..]);
        digest.BlockUpdate(hPubKey);
        digest.DoFinal(state.H.Span);

        return state;
    }
}