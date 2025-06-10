using NLightning.Domain.Crypto.Hashes;
using NLightning.Tests.Utils.Mocks.Interfaces;

namespace NLightning.Tests.Utils.Mocks;

public class FakeSha256 : ISha256, ITestSha256
{
    public void AppendData(ReadOnlySpan<byte> data)
    {
    }

    public void GetHashAndReset(Span<byte> hash)
    {
        var result = GetHashAndReset();
        result.CopyTo(hash);
    }

    public virtual byte[] GetHashAndReset()
    {
        return new byte[32];
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}