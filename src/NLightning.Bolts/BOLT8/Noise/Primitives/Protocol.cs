using System.Text;

namespace NLightning.Bolts.BOLT8.Noise.Primitives;

using Dhs;
using MessagePatterns;
using NLightning.Bolts.BOLT8.Noise.States;

/// <summary>
/// A concrete Noise protocol (Noise_XK_secp256k1_ChaChaPoly_SHA256).
/// </summary>
public sealed class Protocol
{
	/// <summary>
	/// Maximum size of the Noise protocol message in bytes.
	/// </summary>
	public const int MAX_MESSAGE_LENGTH = 65535;

	/// <summary>
	/// Fixed Prologue for the Lightning Network.
	/// </summary>
	private const string PROLOGUE = "lightning";

	/// <summary>
	/// Gets the handshake pattern.
	/// </summary>
	public static HandshakePattern HandshakePattern => HandshakePattern.XK;

	internal static byte[] Name => Encoding.ASCII.GetBytes("Noise_XK_secp256k1_ChaChaPoly_SHA256");

	/// <summary>
	/// Creates an initial <see cref="IHandshakeState"/>.
	/// </summary>
	/// <param name="initiator">A boolean indicating the initiator or responder role.</param>
	/// <param name="prologue">
	/// A byte sequence which may be zero-length, or which may contain
	/// context information that both parties want to confirm is identical.
	/// </param>
	/// <param name="s">The local static private key (optional).</param>
	/// <param name="rs">The remote party's static public key (optional).</param>
	/// <param name="psks">The collection of zero or more 32-byte pre-shared secret keys.</param>
	/// <returns>The initial handshake state.</returns>
	/// <exception cref="ArgumentException">
	/// Thrown if any of the following conditions is satisfied:
	/// <para>- <paramref name="s"/> is not a valid DH private key.</para>
	/// <para>- <paramref name="rs"/> is not a valid DH public key.</para>
	/// <para>- <see cref="HandshakePattern"/> requires the <see cref="IHandshakeState"/>
	/// to be initialized with local and/or remote static key,
	/// but <paramref name="s"/> and/or <paramref name="rs"/> is null.</para>
	/// <para>- One or more pre-shared keys are not 32 bytes in length.</para>
	/// <para>- Number of pre-shared keys does not match the number of PSK modifiers.</para>
	/// <para>- Fallback modifier is present (fallback can only be applied by calling
	/// the <see cref="IHandshakeState.Fallback"/> method on existing handshake state).</para>
	/// </exception>
	internal HandshakeState<Secp256k1> Create(bool initiator, byte[]? s = default, byte[]? rs = default)
	{
		var prologue = Encoding.ASCII.GetBytes(PROLOGUE);

		return new HandshakeState<Secp256k1>(this, initiator, prologue, s, rs);
	}

	private ref struct StringSplitter(ReadOnlySpan<char> s, char separator)
	{
		private ReadOnlySpan<char> s = s;
		private readonly char separator = separator;

		public ReadOnlySpan<char> Next()
		{
			int index = s.IndexOf(separator);

			if (index > 0)
			{
				var next = s[..index];
				s = s[(index + 1)..];

				return next;
			}
			else
			{
				var next = s;
				s = [];

				return next;
			}
		}
	}
}