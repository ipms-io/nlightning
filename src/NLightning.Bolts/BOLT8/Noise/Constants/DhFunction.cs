// namespace NLightning.Bolts.BOLT8.Noise.Constants;

// /// <summary>
// /// Constants representing the available DH functions.
// /// </summary>
// public sealed class DhFunction(string name)
// {
// 	public const string SECP256K1 = "secp256k1";

// 	/// <summary>
// 	/// The Secp256k1 DH function (
// 	/// <see href="https://github.com/lightning/bolts/blob/master/08-transport.md">Bolt 8</see>).
// 	/// </summary>
// 	public static readonly DhFunction Secp256k1 = new("secp256k1");

// 	/// <summary>
// 	/// Returns a string that represents the current object.
// 	/// </summary>
// 	/// <returns>The name of the current DH function.</returns>
// 	public override string ToString() => name;

// 	internal static DhFunction Parse(ReadOnlySpan<char> s)
// 	{
// 		return s switch
// 		{
// 			var _ when s.SequenceEqual(Secp256k1.ToString().AsSpan()) => Secp256k1,
// 			_ => throw new ArgumentException("Unknown DH function.", nameof(s)),
// 		};
// 	}
// }