namespace NLightning.Bolts.BOLT8.Noise.Constants;

/// <summary>
/// Constants representing the available cipher functions.
/// </summary>
public sealed class CipherFunction(string name)
{
	/// <summary>
	/// AEAD_CHACHA20_POLY1305 from <see href="https://tools.ietf.org/html/rfc7539">RFC 7539</see>.
	/// </summary>
	public static readonly CipherFunction CHACHA_POLY = new("ChaChaPoly");

	/// <summary>
	/// Returns a string that represents the current object.
	/// </summary>
	/// <returns>The name of the current cipher function.</returns>
	public override string ToString() => name;

	internal static CipherFunction Parse(ReadOnlySpan<char> s)
	{
		return s switch
		{
			var _ when s.SequenceEqual(CHACHA_POLY.ToString().AsSpan()) => CHACHA_POLY,
			_ => throw new ArgumentException("Unknown cipher function.", nameof(s)),
		};
	}
}