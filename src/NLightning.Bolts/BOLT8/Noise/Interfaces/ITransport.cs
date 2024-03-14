namespace NLightning.Bolts.BOLT8.Noise.Interfaces;

/// <summary>
/// A pair of <see href="https://noiseprotocol.org/noise.html#the-cipherstate-object">CipherState</see>
/// objects for encrypting transport messages.
/// </summary>
public interface ITransport : IDisposable
{
    // TODO: Write documentation
    int WriteMessage(ReadOnlySpan<byte> payload, Span<byte> messageBuffer);
    int ReadMessageLength(ReadOnlySpan<byte> message);
    int ReadMessagePayload(ReadOnlySpan<byte> message, Span<byte> payloadBuffer);
}