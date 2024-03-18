// using System.Text;

// namespace NLightning.Bolts.BOLT8.Primitives;

// using Constants;
// using Interfaces;
// using MessagePatterns;
// using States;

// /// <summary>
// /// A concrete Noise protocol (Noise_XK_secp256k1_ChaChaPoly_SHA256).
// /// </summary>
// internal sealed class Protocol(IDh? dh = null)
// {
// 	private readonly IDh? _dh = dh;

// 	/// <summary>
// 	/// Gets the handshake pattern.
// 	/// </summary>
// 	internal static HandshakePattern HandshakePattern => HandshakePattern.XK;

// 	/// <summary>
// 	/// Creates an initial <see cref="IHandshakeState"/>.
// 	/// </summary>
// 	/// <param name="initiator">A boolean indicating the initiator or responder role.</param>
// 	/// <param name="prologue">
// 	/// A byte sequence which may be zero-length, or which may contain
// 	/// context information that both parties want to confirm is identical.
// 	/// </param>
// 	/// <param name="s">The local static private key (optional).</param>
// 	/// <param name="rs">The remote party's static public key (optional).</param>
// 	/// <param name="psks">The collection of zero or more 32-byte pre-shared secret keys.</param>
// 	/// <returns>The initial handshake state.</returns>
// 	/// <exception cref="ArgumentException">
// 	/// Thrown if any of the following conditions is satisfied:
// 	/// <para>- <paramref name="s"/> is not a valid DH private key.</para>
// 	/// <para>- <paramref name="rs"/> is not a valid DH public key.</para>
// 	/// <para>- <see cref="HandshakePattern"/> requires the <see cref="IHandshakeState"/>
// 	/// to be initialized with local and/or remote static key,
// 	/// but <paramref name="s"/> and/or <paramref name="rs"/> is null.</para>
// 	/// <para>- One or more pre-shared keys are not 32 bytes in length.</para>
// 	/// <para>- Number of pre-shared keys does not match the number of PSK modifiers.</para>
// 	/// <para>- Fallback modifier is present (fallback can only be applied by calling
// 	/// the <see cref="IHandshakeState.Fallback"/> method on existing handshake state).</para>
// 	/// </exception>
// 	public IHandshakeState Create(bool initiator, ReadOnlySpan<byte> s = default, ReadOnlySpan<byte> rs = default)
// 	{
// 		var prologue = ProtocolConstants.PROLOGUE;

// 		return new HandshakeState(this, initiator, prologue, s, rs, _dh);
// 	}
// }