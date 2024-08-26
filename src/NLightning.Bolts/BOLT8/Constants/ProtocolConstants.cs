using System.Text;

namespace NLightning.Bolts.BOLT8.Constants;

internal static class ProtocolConstants
{
    /// <summary>
    /// Maximum size of the Noise protocol message in bytes.
    /// </summary>
    public const int MAX_MESSAGE_LENGTH = 65535;

    /// <summary>
    /// The size of the Message Header.
    /// </summary>
    public const int MESSAGE_HEADER_SIZE = 18;

    /// <summary>
    /// The byte[] representation of the Prologue for the Lightning Network.
    /// </summary>
    public static readonly byte[] PROLOGUE = Encoding.ASCII.GetBytes("lightning");

    /// <summary>
    /// The byte[] representations of the name of the Noise protocol.
    /// </summary>
    public static readonly byte[] NAME = Encoding.ASCII.GetBytes("Noise_XK_secp256k1_ChaChaPoly_SHA256");

    /// <summary>
    /// Empty message used throughout the Noise protocol.
    /// </summary>
    public static readonly byte[] EMPTY_MESSAGE = [];
}