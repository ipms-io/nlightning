namespace NLightning.Domain.Transport.Interfaces;

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
    int WriteMessage(ReadOnlySpan<byte> payload, Span<byte> messageBuffer);

    int ReadMessageLength(ReadOnlySpan<byte> lc);
    int ReadMessagePayload(ReadOnlySpan<byte> message, Span<byte> payloadBuffer);
}