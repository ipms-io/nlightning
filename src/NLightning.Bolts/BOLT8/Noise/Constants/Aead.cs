namespace NLightning.Bolts.BOLT8.Noise.Constants;

/// <summary>
/// AEAD constants.
/// </summary>
internal static class Aead
{
	/// <summary>
	/// Secret key size in bytes.
	/// </summary>
	public const int KEY_SIZE = 32;

	/// <summary>
	/// Nonce size in bytes.
	/// </summary>
	public const int NONCE_SIZE = 12;

	/// <summary>
	/// Authentication tag size in bytes.
	/// </summary>
	public const int TAG_SIZE = 16;
}