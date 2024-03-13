namespace NLightning.Bolts.BOLT8.Noise.Interfaces;

using Primitives;

/// <summary>
/// DH functions (and an associated constant).
/// </summary>
internal interface IDh
{
	/// <summary>
	/// A constant specifying the size in bytes of public keys and DH outputs.
	/// For security reasons, DhLen must be 32 or greater.
	/// </summary>
	int PrivLen { get; }

	/// <summary>
	/// A constant specifying the size in bytes of public keys and DH outputs.
	/// For security reasons, DhLen must be 32 or greater.
	/// </summary>
	int PubLen { get; }

	/// <summary>
	/// Performs a Diffie-Hellman calculation between the private
	/// key in keyPair and the publicKey and writes an output
	/// sequence of bytes of length DhLen into sharedKey parameter.
	/// </summary>
	void Dh(NBitcoin.Key k, ReadOnlySpan<byte> rk, Span<byte> sharedKey);

	/// <summary>
	/// Generates a new Diffie-Hellman key pair.
	/// </summary>
	KeyPair GenerateKeyPair();

	/// <summary>
	/// Generates a Diffie-Hellman key pair from the specified private key.
	/// </summary>
	KeyPair GenerateKeyPair(ReadOnlySpan<byte> privateKey);
}