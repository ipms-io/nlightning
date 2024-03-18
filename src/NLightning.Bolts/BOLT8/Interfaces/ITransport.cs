namespace NLightning.Bolts.BOLT8.Interfaces;

/// <summary>
/// A pair of <see href="https://noiseprotocol.org/noise.html#the-cipherstate-object">CipherState</see> objects for encrypting transport messages.
/// </summary>
internal interface ITransport : IDisposable
{
    /// <summary>
    /// Encrypts the <paramref name="payload"/> and writes the result into <paramref name="messageBuffer"/>.
    /// </summary>
    /// <param name="payload">The payload to encrypt.</param>
    /// <param name="messageBuffer">The buffer for the encrypted message.</param>
    /// <returns>The ciphertext size in bytes.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the current instance has already been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the responder has attempted to write a message to a one-way stream.</exception>
    /// <exception cref="ArgumentException">Thrown if the encrypted payload was greater than <see cref="Protocol.MaxMessageLength"/> bytes in length, or if the output buffer did not have enough space to hold the ciphertext.</exception>
    int WriteMessage(ReadOnlySpan<byte> payload, Span<byte> messageBuffer);

    int ReadMessageLength(ReadOnlySpan<byte> lc);
    int ReadMessagePayload(ReadOnlySpan<byte> message, Span<byte> payloadBuffer);
}