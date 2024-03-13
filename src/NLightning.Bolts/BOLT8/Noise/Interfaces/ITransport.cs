namespace NLightning.Bolts.BOLT8.Noise.Interfaces;

/// <summary>
/// A pair of <see href="https://noiseprotocol.org/noise.html#the-cipherstate-object">CipherState</see>
/// objects for encrypting transport messages.
/// </summary>
public interface ITransport : IDisposable
{
    /// <summary>
    /// Encrypts the <paramref name="payload"/> and writes the result into <paramref name="messageBuffer"/>.
    /// </summary>
    /// <param name="payload">The payload to encrypt.</param>
    /// <param name="messageBuffer">The buffer for the encrypted message.</param>
    /// <returns>The ciphertext size in bytes.</returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the current instance has already been disposed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the responder has attempted to write a message to a one-way stream.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the encrypted payload was greater than <see cref="Protocol.MaxMessageLength"/>
    /// bytes in length, or if the output buffer did not have enough space to hold the ciphertext.
    /// </exception>
    int WriteMessage(ReadOnlySpan<byte> payload, Span<byte> messageBuffer);

    /// <summary>
    /// Decrypts the <paramref name="message"/> and writes the result into <paramref name="payloadBuffer"/>.
    /// </summary>
    /// <param name="message">The message to decrypt.</param>
    /// <param name="payloadBuffer">The buffer for the decrypted payload.</param>
    /// <returns>The plaintext size in bytes.</returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the current instance has already been disposed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the initiator has attempted to read a message from a one-way stream.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the message was greater than <see cref="Protocol.MaxMessageLength"/>
    /// bytes in length, or if the output buffer did not have enough space to hold the plaintext.
    /// </exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">
    /// Thrown if the decryption of the message has failed.
    /// </exception>
    int ReadMessage(ReadOnlySpan<byte> message, Span<byte> payloadBuffer);

    /// <summary>
    /// Updates the symmetric key used to encrypt transport messages from
    /// initiator to responder using a one-way function, so that a compromise
    /// of keys will not decrypt older messages.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the current instance has already been disposed.
    /// </exception>
    void RekeyInitiatorToResponder();

    /// <summary>
    /// Updates the symmetric key used to encrypt transport messages from
    /// responder to initiator using a one-way function, so that a compromise
    /// of keys will not decrypt older messages.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the current instance has already been disposed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the current instance is a one-way stream.
    /// </exception>
    void RekeyResponderToInitiator();
}