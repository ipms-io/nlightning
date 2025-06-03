using NLightning.Domain.Protocol.ValueObjects;

namespace NLightning.Infrastructure.Serialization.Tests.ValueObjects;

using Domain.ValueObjects;
using Infrastructure.Serialization.ValueObjects;

public class BigSizeTypeSerializerTests
{
    [Fact]
    public async Task Given_VectorInputs_When_DeserializeBigSize_Then_ResultIsKnown()
    {
        var testVectors = ReadTestVectors("Vectors/BigSize.txt").Where(x => x.Error == null);
        var bigSizeSerializer = new BigSizeTypeSerializer();

        foreach (var testVector in testVectors)
        {
            // Given
            using var memoryStream = new MemoryStream(testVector.Bytes);

            // When
            var bigSizeValue = await bigSizeSerializer.DeserializeAsync(memoryStream);

            // Then
            Assert.Equal(testVector.Value, bigSizeValue.Value);
        }
    }

    [Fact]
    public async Task Given_VectorInputs_When_DeserializeBigSize_Then_ErrorIsThrown()
    {
        // Arrange
        var testVectors = ReadTestVectors("Vectors/BigSize.txt").Where(x => x.Error != null);
        var bigSizeSerializer = new BigSizeTypeSerializer();

        foreach (var testVector in testVectors)
        {
            // Arrange
            using var memoryStream = new MemoryStream(testVector.Bytes);

            // Assert
            await Assert.ThrowsAnyAsync<Exception>(Deserialize);
            continue;

            // Act
            Task Deserialize() => bigSizeSerializer.DeserializeAsync(memoryStream);
        }
    }

    [Fact]
    public async Task Given_VectorInputs_When_SerializeBigSize_Then_ResultIsKnown()
    {
        // Arrange
        var testVectors = ReadTestVectors("Vectors/BigSize.txt").Where(x => x.Error == null);
        var bigSizeSerializer = new BigSizeTypeSerializer();

        foreach (var testVector in testVectors)
        {
            // Arrange
            using var memoryStream = new MemoryStream();

            // Act
            await bigSizeSerializer.SerializeAsync(new BigSize(testVector.Value), memoryStream);

            // Assert
            Assert.Equal(testVector.Bytes, memoryStream.ToArray());
        }
    }

    private class TestVector
    {
        public ulong Value { get; set; }
        public byte[] Bytes { get; set; } = [];
        public string? Error { get; set; }
    }

    private static List<TestVector> ReadTestVectors(string filePath)
    {
        var testVectors = new List<TestVector>();
        TestVector? currentVector = null;

        foreach (var line in File.ReadLines(filePath))
        {
            if (line.StartsWith("Value: "))
            {
                currentVector = new TestVector
                {
                    Value = ulong.Parse(line[7..])
                };
            }
            else if (line.StartsWith("Bytes: "))
            {
                if (currentVector == null)
                {
                    throw new InvalidOperationException("Bytes line without Value line");
                }

                currentVector.Bytes = Convert.FromHexString(line[7..]);
            }
            else if (line.StartsWith("Error: "))
            {
                if (currentVector == null)
                {
                    throw new InvalidOperationException("Bytes line without Value line");
                }

                if (currentVector.Bytes == null)
                {
                    throw new InvalidOperationException("Error line without Bytes line");
                }

                var error = line[7..];
                if (!string.IsNullOrEmpty(error))
                {
                    currentVector.Error = error;
                }

                testVectors.Add(currentVector);
            }
        }

        return testVectors;
    }
}