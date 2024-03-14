// namespace NLightning.Bolts.BOLT8.Noise.Constants;

// /// <summary>
// /// Constants representing the available hash functions.
// /// </summary>
// public sealed class HashFunction(string name)
// {
// 	/// <summary>
// 	/// SHA-256 from <see href="https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.180-4.pdf">FIPS 180-4</see>.
// 	/// </summary>
// 	public static readonly HashFunction Sha256 = new("SHA256");

// 	/// <summary>
// 	/// Returns a string that represents the current object.
// 	/// </summary>
// 	/// <returns>The name of the current hash function.</returns>
// 	public override string ToString() => name;

// 	internal static HashFunction Parse(ReadOnlySpan<char> s)
// 	{
// 		return s switch
// 		{
// 			var _ when s.SequenceEqual(Sha256.ToString().AsSpan()) => Sha256,
// 			_ => throw new ArgumentException("Unknown hash function.", nameof(s)),
// 		};
// 	}
// }