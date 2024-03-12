namespace NLightning.Bolts.BOLT8.Noise;

/// <summary>
/// A set of parameters used to instantiate an initial
/// <see cref="IHandshakeState"/> from a concrete <see cref="Protocol"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ProtocolConfig"/> class.
/// </remarks>
/// <param name="initiator">A boolean indicating the initiator or responder role.</param>
/// <param name="prologue">
/// A byte sequence which may be zero-length, or which may contain
/// context information that both parties want to confirm is identical.
/// </param>
/// <param name="s">The local static private key.</param>
/// <param name="rs">The remote party's static public key.</param>
/// <param name="psks">The collection of zero or more 32-byte pre-shared secret keys.</param>
public sealed class ProtocolConfig(
	bool initiator = default,
	byte[] prologue = default,
	byte[] s = default,
	byte[] rs = default,
	IEnumerable<byte[]> psks = default)
{

	/// <summary>
	/// Gets or sets the initiator or responder role.
	/// </summary>
	public bool Initiator { get; set; } = initiator;

	/// <summary>
	/// Gets or sets the prologue (a byte sequence which may be
	/// zero-length, or which may contain context information
	/// that both parties want to confirm is identical).
	/// </summary>
	public byte[] Prologue { get; set; } = prologue;

	/// <summary>
	/// Gets or sets the local static private key.
	/// </summary>
	public byte[] LocalStatic { get; set; } = s;

	/// <summary>
	/// Gets or sets the remote party's static public key.
	/// </summary>
	public byte[] RemoteStatic { get; set; } = rs;

	/// <summary>
	/// Gets or sets the collection of zero or more 32-byte pre-shared secret keys.
	/// </summary>
	public IEnumerable<byte[]> PreSharedKeys { get; set; } = psks;
}