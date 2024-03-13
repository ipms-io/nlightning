using System.Reflection;
using System.Text;

namespace NLightning.Bolts.BOLT8.Noise.Primitives;

using NLightning.Bolts.BOLT8.Noise.Ciphers;
using NLightning.Bolts.BOLT8.Noise.Constants;
using NLightning.Bolts.BOLT8.Noise.Hashes;
using NLightning.Bolts.BOLT8.Noise.Interfaces;
using NLightning.Bolts.BOLT8.Noise.MessagePatterns;
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

	private static readonly Dictionary<string, HandshakePattern> _patterns = typeof(HandshakePattern).GetTypeInfo()
		.DeclaredFields
		.Where(field => field.IsPublic && field.IsStatic && field.FieldType == typeof(HandshakePattern))?
		.ToDictionary(field => field.Name, field => (HandshakePattern?)field.GetValue(null) ?? throw new InvalidOperationException("Failed to initialize Noise handshake patterns."))
		?? throw new InvalidOperationException("Failed to initialize Noise handshake patterns.");

	/// <summary>
	/// Initializes a new instance of the <see cref="Protocol"/>
	/// class using ChaChaPoly, 25519, and SHA256 functions.
	/// </summary>
	/// <param name="handshakePattern">The handshake pattern (e.q. NX or IK).</param>
	/// <param name="modifiers">The combination of pattern modifiers (e.q. empty, psk0, or psk1+psk2).</param>
	/// <exception cref="ArgumentNullException">
	/// Thrown if the <paramref name="handshakePattern"/> is null.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// Thrown if <paramref name="modifiers"/> does not represent a valid combination of pattern modifiers.
	/// </exception>
	public Protocol() : this(HandshakePattern.XK, CipherFunction.CHACHA_POLY, HashFunction.Sha256)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="Protocol"/> class.
	/// </summary>
	/// <param name="handshakePattern">The handshake pattern (e.q. NX or IK).</param>
	/// <param name="cipher">The cipher function (AESGCM or ChaChaPoly).</param>
	/// <param name="hash">The hash function (SHA256, SHA512, BLAKE2s, or BLAKE2b).</param>
	/// <param name="modifiers">The combination of pattern modifiers (e.q. empty, psk0, or psk1+psk2).</param>
	/// <exception cref="ArgumentNullException">
	/// Thrown if either <paramref name="handshakePattern"/>,
	/// <paramref name="cipher"/>, or <paramref name="hash"/> is null.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// Thrown if <paramref name="modifiers"/> does not represent a valid combination of pattern modifiers.
	/// </exception>
	public Protocol(
		HandshakePattern handshakePattern,
		CipherFunction cipher,
		HashFunction hash)
	{
		Exceptions.ThrowIfNull(handshakePattern, nameof(handshakePattern));
		Exceptions.ThrowIfNull(cipher, nameof(cipher));
		Exceptions.ThrowIfNull(hash, nameof(hash));

		HandshakePattern = handshakePattern;
		Cipher = cipher;
		Dh = DhFunction.Secp256k1;
		Hash = hash;

		Name = GetName();
	}

	/// <summary>
	/// Gets the handshake pattern.
	/// </summary>
	public HandshakePattern HandshakePattern { get; }

	/// <summary>
	/// Gets the cipher function.
	/// </summary>
	public CipherFunction Cipher { get; }

	/// <summary
	/// >Gets the Diffie-Hellman function.
	/// </summary>
	public DhFunction Dh { get; }

	/// <summary>
	/// Gets the hash function.
	/// </summary>
	public HashFunction Hash { get; }

	internal byte[] Name { get; }

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
	public IHandshakeState Create(
		bool initiator,
		byte[]? s = default,
		byte[]? rs = default)
	{
		var prologue = Encoding.ASCII.GetBytes(PROLOGUE);

		return new HandshakeState<ChaCha20Poly1305, Secp256k1, SHA256>(this, initiator, prologue, s, rs);
	}

	private byte[] GetName()
	{
		var protocolName = new StringBuilder("Noise");

		protocolName.Append('_');
		protocolName.Append(HandshakePattern.Name);

		protocolName.Append('_');
		protocolName.Append(Dh);

		protocolName.Append('_');
		protocolName.Append(Cipher);

		protocolName.Append('_');
		protocolName.Append(Hash);

		return Encoding.ASCII.GetBytes(protocolName.ToString());
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