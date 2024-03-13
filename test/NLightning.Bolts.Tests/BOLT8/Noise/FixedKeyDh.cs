using NLightning.Bolts.BOLT8.Noise.Constants;
using NLightning.Bolts.BOLT8.Noise.Interfaces;
using NLightning.Bolts.BOLT8.Noise.Primitives;

namespace NLightning.Bolts.Tests.BOLT8.Noise;

internal class FixedKeyDh(byte[] privateKey) : IDh
{
	private readonly IDh _dh = new Secp256k1();

	public int PrivLen => _dh.PrivLen;
	public int PubLen => _dh.PubLen;

	public KeyPair GenerateKeyPair()
	{
		return _dh.GenerateKeyPair(privateKey);
	}

	public KeyPair GenerateKeyPair(ReadOnlySpan<byte> privKey)
	{
		return _dh.GenerateKeyPair(privKey);
	}

	public void Dh(NBitcoin.Key k, ReadOnlySpan<byte> rk, Span<byte> sharedKey)
	{
		_dh.Dh(k, rk, sharedKey);
	}
}