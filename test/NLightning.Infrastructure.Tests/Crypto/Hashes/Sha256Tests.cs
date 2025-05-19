namespace NLightning.Infrastructure.Tests.Crypto.Hashes;

using Infrastructure.Crypto.Hashes;

/// <summary>
/// Test our SHA256 implementation against the test vector available at
/// <see href="https://csrc.nist.gov/projects/cryptographic-algorithm-validation-program/secure-hashing">NIST</see>.
/// </summary>
public class Sha256Tests
{
    [Fact]
    public void Given_NistVectorInputs_When_DataIsHashed_Then_ResultIsKnown()
    {
        var testVectors = ReadTestVectors("Crypto/Vectors/SHA256LongMsg.rsp");
        using var sha256 = new Sha256();
        Span<byte> result = new byte[32];

        foreach (var vector in testVectors)
        {
            sha256.AppendData(vector.Msg);
            sha256.GetHashAndReset(result);

            Assert.Equal(vector.Md, result.ToArray());
        }
    }

    private class TestVector(int len)
    {
        public int Len { get; } = len;
        public byte[]? Msg { get; set; }
        public byte[]? Md { get; set; }
    }

    private static List<TestVector> ReadTestVectors(string filePath)
    {
        var testVectors = new List<TestVector>();
        TestVector? currentVector = null;

        foreach (var line in File.ReadLines(filePath))
        {
            if (line.StartsWith("Len = "))
            {
                currentVector = new TestVector(int.Parse(line[6..]));
            }
            else if (line.StartsWith("Msg = "))
            {
                if (currentVector == null)
                {
                    throw new InvalidOperationException("Msg line without Len line");
                }

                currentVector.Msg = Convert.FromHexString(line[6..]);

                if (currentVector.Msg.Length != currentVector.Len / 8)
                {
                    throw new InvalidOperationException("Msg length does not match Len");
                }
            }
            else if (line.StartsWith("MD = "))
            {
                if (currentVector == null)
                {
                    throw new InvalidOperationException("MD line without Len line");
                }

                if (currentVector.Msg == null)
                {
                    throw new InvalidOperationException("MD line without Msg line");
                }

                currentVector.Md = Convert.FromHexString(line[5..]);
                testVectors.Add(currentVector);
            }
        }

        return testVectors;
    }
}