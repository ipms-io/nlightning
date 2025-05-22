namespace NLightning.Domain.Crypto.Hashes;

public interface ISha256 : IDisposable
{
    void AppendData(ReadOnlySpan<byte> data);
    void GetHashAndReset(Span<byte> hash);
}