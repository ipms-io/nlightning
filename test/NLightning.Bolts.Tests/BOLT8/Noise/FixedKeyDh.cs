using NLightning.Bolts.BOLT8.Noise;
using NLightning.Bolts.BOLT8.Noise.Constants;
using NLightning.Bolts.BOLT8.Noise.Interfaces;
using NLightning.Bolts.BOLT8.Noise.Primitives;

namespace NLightning.Bolts.Tests.BOLT8.Noise;

internal class FixedKeyDh(byte[] privateKey) : IDh
{
	private static readonly Curve25519 dh = new();
	private readonly byte[] privateKey = privateKey;

	public int DhLen => dh.DhLen;

	public KeyPair GenerateKeyPair()
	{
		var publicKey = new byte[DhLen];
		_ = Libsodium.crypto_scalarmult_curve25519_base(publicKey, privateKey);

		return new KeyPair(privateKey, publicKey);
	}

	public KeyPair GenerateKeyPair(ReadOnlySpan<byte> privateKey)
	{
		return dh.GenerateKeyPair(privateKey);
	}

	public void Dh(KeyPair keyPair, ReadOnlySpan<byte> publicKey, Span<byte> sharedKey)
	{
		dh.Dh(keyPair, publicKey, sharedKey);
	}
}