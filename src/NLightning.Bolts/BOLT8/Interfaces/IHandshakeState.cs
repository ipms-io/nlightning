namespace NLightning.Bolts.BOLT8.Interfaces;

using Primitives;

/// <summary>
/// A <see href="https://noiseprotocol.org/noise.html#the-handshakestate-object">HandshakeState</see> object contains a <see href="https://noiseprotocol.org/noise.html#the-symmetricstate-object">SymmetricState</see> plus the local and remote keys, a boolean indicating the initiator or responder role, and the remaining portion of the handshake pattern.
/// </summary>
internal interface IHandshakeState : IDisposable
{
    /// <summary>
    /// Performs the next step of the handshake, encrypts the <paramref name="payload"/>, and writes the result into <paramref name="messageBuffer"/>.
    /// The result is undefined if the <paramref name="payload"/> and <paramref name="messageBuffer"/> overlap.
    /// </summary>
    /// <param name="payload">The payload to encrypt.</param>
    /// <param name="messageBuffer">The buffer for the encrypted message.</param>
    /// <returns>The tuple containing the ciphertext size in bytes, the handshake hash, and the <see cref="ITransport"/> object for encrypting transport messages. If the handshake is still in progress, the handshake hash and the transport will both be null.</returns>
    (int, byte[]?, Transport?) WriteMessage(ReadOnlySpan<byte> payload, Span<byte> messageBuffer);

    /// <summary>
    /// Performs the next step of the handshake, decrypts the <paramref name="message"/>, and writes the result into <paramref name="payloadBuffer"/>.
    /// The result is undefined if the <paramref name="message"/> and <paramref name="payloadBuffer"/> overlap.
    /// </summary>
    /// <param name="message">The message to decrypt.</param>
    /// <param name="payloadBuffer">The buffer for the decrypted payload.</param>
    /// <returns>
    /// The tuple containing the plaintext size in bytes, the handshake hash, and the <see cref="ITransport"/> object for encrypting transport messages. If the handshake is still in progress, the handshake hash and the transport will both be null.</returns>
    (int, byte[]?, Transport?) ReadMessage(ReadOnlySpan<byte> message, Span<byte> payloadBuffer);
}