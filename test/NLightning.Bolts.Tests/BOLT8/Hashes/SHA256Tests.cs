using NLightning.Bolts.BOLT8.Hashes;

namespace NLightning.Bolts.Tests.BOLT8.Hashes;

using static Utils.TestUtils;

/// <summary>
/// Test our SHA256 implementation against the test vector available at
/// <see href="https://csrc.nist.gov/projects/cryptographic-algorithm-validation-program/secure-hashing">NIST</see>.
/// </summary>
public partial class SHA256Tests
{
    [Fact]
    public void Given_NistVectorInputs_When_DataIsHashed_Then_ResultIsKnown()
    {
        var hasher = new SHA256();
        var testVectors = ReadTestVectors("BOLT8/Vectors/SHA256LongMsg.rsp");
        var result = new byte[32];

        foreach (var vector in testVectors)
        {
            hasher.AppendData(vector.Msg);
            hasher.GetHashAndReset(result);

            Assert.Equal(vector.MD, result);
        }
    }

    private class TestVector
    {
        public int Len { get; set; }
        public byte[]? Msg { get; set; }
        public byte[]? MD { get; set; }
    }

    private static List<TestVector> ReadTestVectors(string filePath)
    {
        var testVectors = new List<TestVector>();
        TestVector? currentVector = null;

        foreach (var line in File.ReadLines(filePath))
        {
            if (line.StartsWith("Len = "))
            {
                currentVector = new TestVector
                {
                    Len = int.Parse(line[6..])
                };
            }
            else if (line.StartsWith("Msg = "))
            {
                if (currentVector == null)
                {
                    throw new InvalidOperationException("Msg line without Len line");
                }

                currentVector.Msg = GetBytes(line[6..]);

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

                currentVector.MD = GetBytes(line[5..]);
                testVectors.Add(currentVector);
            }
        }

        return testVectors;
    }
}