using NLightning.Bolts.BOLT8.Noise.Primitives;

namespace NLightning.Bolts.BOLT8.Noise.Interfaces;

/// <summary>
/// A <see href="https://noiseprotocol.org/noise.html#the-handshakestate-object">HandshakeState</see>
/// object contains a <see href="https://noiseprotocol.org/noise.html#the-symmetricstate-object">SymmetricState</see>
/// plus the local and remote keys (any of which may be empty),
/// a boolean indicating the initiator or responder role, and
/// the remaining portion of the handshake pattern.
/// </summary>
public interface IHandshakeState : IDisposable
{
    /// <summary>
    /// The remote party's static public key.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the current instance has already been disposed.
    /// </exception>
    ReadOnlySpan<byte> RemoteStaticPublicKey { get; }

    /// <summary>
    /// Converts an Alice-initiated pattern to a Bob-initiated pattern.
    /// The only fallback pattern currently supported is XXfallback.
    /// PSK modifiers are currently not supported with fallback protocols.
    /// </summary>
    /// <param name="protocol">A concrete Noise protocol (e.g. Noise_XXfallback_25519_AESGCM_BLAKE2b).</param>
    /// <param name="config">A set of parameters used to instantiate a <see cref="IHandshakeState"/>.</param>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the current instance has already been disposed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown if either <paramref name="protocol"/> or <paramref name="config"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="protocol"/> is not XXfallback,
    /// or if the provided local static private key is empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Throw if the initial handshake pattern is Bob-initiated, or if this
    /// method was not called immediately after the first handshake message.
    /// </exception>
    void Fallback(Protocol protocol, ProtocolConfig config);

    /// <summary>
    /// Performs the next step of the handshake,
    /// encrypts the <paramref name="payload"/>,
    /// and writes the result into <paramref name="messageBuffer"/>.
    /// The result is undefined if the <paramref name="payload"/>
    /// and <paramref name="messageBuffer"/> overlap.
    /// </summary>
    /// <param name="payload">The payload to encrypt.</param>
    /// <param name="messageBuffer">The buffer for the encrypted message.</param>
    /// <returns>
    /// The tuple containing the ciphertext size in bytes,
    /// the handshake hash, and the <see cref="ITransport"/>
    /// object for encrypting transport messages. If the
    /// handshake is still in progress, the handshake hash
    /// and the transport will both be null.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the current instance has already been disposed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the call to <see cref="ReadMessage"/> was expected
    /// or the handshake has already been completed.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the output was greater than <see cref="Protocol.MaxMessageLength"/>
    /// bytes in length, or if the output buffer did not have enough space to hold the ciphertext.
    /// </exception>
    (int BytesWritten, byte[] HandshakeHash, ITransport Transport) WriteMessage(
        ReadOnlySpan<byte> payload,
        Span<byte> messageBuffer
    );

    /// <summary>
    /// Performs the next step of the handshake,
    /// decrypts the <paramref name="message"/>,
    /// and writes the result into <paramref name="payloadBuffer"/>.
    /// The result is undefined if the <paramref name="message"/>
    /// and <paramref name="payloadBuffer"/> overlap.
    /// </summary>
    /// <param name="message">The message to decrypt.</param>
    /// <param name="payloadBuffer">The buffer for the decrypted payload.</param>
    /// <returns>
    /// The tuple containing the plaintext size in bytes,
    /// the handshake hash, and the <see cref="ITransport"/>
    /// object for encrypting transport messages. If the
    /// handshake is still in progress, the handshake hash
    /// and the transport will both be null.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the current instance has already been disposed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the call to <see cref="WriteMessage"/> was expected
    /// or the handshake has already been completed.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the message was greater than <see cref="Protocol.MaxMessageLength"/>
    /// bytes in length, or if the output buffer did not have enough space to hold the plaintext.
    /// </exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">
    /// Thrown if the decryption of the message has failed.
    /// </exception>
    (int BytesRead, byte[] HandshakeHash, ITransport Transport) ReadMessage(
        ReadOnlySpan<byte> message,
        Span<byte> payloadBuffer
    );
}