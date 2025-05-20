// Based on Noise.NET by Nemanja Mijailovic https://github.com/Metalnem/noise

namespace NLightning.Infrastructure.Transport.Handshake.Enums;

/// <summary>
/// The smallest unit of the Noise handshake language.
/// </summary>
internal enum Token
{
    /// <summary>
    /// Sender generates an ephemeral key pair and stores it
    /// Writes the ephemeral public key as cleartext into the buffer
    /// Hashes the public key along with the old h to derive a new h.
    /// </summary>
    E,

    /// <summary>
    /// The sender writes its static public key from the s variable
    /// into the message buffer, encrypting it if k is non-empty,
    /// and hashes the output along with the old h to derive a new h.
    /// </summary>
    S,

    /// <summary>
    /// A DH is performed between the initiator's ephemeral private key and
    /// the responder's ephemeral public key. The result is hashed along
    /// with the old ck to derive a new ck and k, and n is set to zero.
    /// </summary>
    EE,

    /// <summary>
    /// A DH is performed between the initiator's static private key and
    /// the responder's ephemeral public key. The result is hashed along
    /// with the old ck to derive a new ck and k, and n is set to zero.
    /// </summary>
    SE,

    /// <summary>
    /// A DH is performed between the initiator's ephemeral private key and
    /// the responder's static public key. The result is hashed along
    /// with the old ck to derive a new ck and k, and n is set to zero.
    /// </summary>
    ES,

    /// <summary>
    /// A DH is performed between the initiator's static private key and
    /// the responder's static public key. The result is hashed along
    /// with the old ck to derive a new ck and k, and n is set to zero.
    /// </summary>
    SS
}