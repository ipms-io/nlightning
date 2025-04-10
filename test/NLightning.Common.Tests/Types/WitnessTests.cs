using System.Runtime.Serialization;

namespace NLightning.Common.Tests.Types;

using Common.Types;

public class WitnessTests
{
    [Fact]
    public async Task Given_ValidWitnessData_When_SerializedAndDeserialized_Then_DataIsPreserved()
    {
        // Given
        var witnessData = new byte[] { 0x01, 0x02, 0x03 };
        var witness = new Witness(witnessData);

        using var memoryStream = new MemoryStream();

        // When
        await witness.SerializeAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var deserializedWitness = await Witness.DeserializeAsync(memoryStream);

        // Then
        Assert.Equal(witnessData, deserializedWitness.WitnessData);
    }

    [Fact]
    public async Task Given_EmptyWitnessData_When_SerializedAndDeserialized_Then_DataIsPreserved()
    {
        // Given
        var witnessData = Array.Empty<byte>();
        var witness = new Witness(witnessData);

        using var memoryStream = new MemoryStream();

        // When
        await witness.SerializeAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var deserializedWitness = await Witness.DeserializeAsync(memoryStream);

        // Then
        Assert.Equal(witnessData, deserializedWitness.WitnessData);
    }

    [Fact]
    public async Task Given_InvalidStream_When_Deserialized_Then_ThrowsSerializationException()
    {
        // Given
        using var memoryStream = new MemoryStream([0x00]);

        // When & Then
        await Assert.ThrowsAsync<SerializationException>(async () => await Witness.DeserializeAsync(memoryStream));
    }

    [Fact]
    public async Task Given_LargeWitnessData_When_SerializedAndDeserialized_Then_DataIsPreserved()
    {
        // Given
        var witnessData = new byte[ushort.MaxValue];
        for (var i = 0; i < witnessData.Length; i++)
        {
            witnessData[i] = (byte)(i % 256);
        }
        var witness = new Witness(witnessData);

        using var memoryStream = new MemoryStream();

        // When
        await witness.SerializeAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var deserializedWitness = await Witness.DeserializeAsync(memoryStream);

        // Then
        Assert.Equal(witnessData, deserializedWitness.WitnessData);
    }
}