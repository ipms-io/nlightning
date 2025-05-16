using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Tests.ValueObjects;

using Domain.ValueObjects;
using Infrastructure.Serialization.ValueObjects;

public class WitnessTypeSerializerTests
{
    
    [Fact]
    public async Task Given_ValidWitnessData_When_SerializedAndDeserialized_Then_DataIsPreserved()
    {
        // Given
        var witnessSerializer = new WitnessTypeSerializer();
        var witnessData = new byte[] { 0x01, 0x02, 0x03 };
        var witness = new Witness(witnessData);

        using var memoryStream = new MemoryStream();

        // When
        await witnessSerializer.SerializeAsync(witness, memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var deserializedWitness = await witnessSerializer.DeserializeAsync(memoryStream);

        // Then
        Assert.Equal(witnessData, deserializedWitness);
    }

    [Fact]
    public async Task Given_EmptyWitnessData_When_SerializedAndDeserialized_Then_DataIsPreserved()
    {
        // Given
        var witnessSerializer = new WitnessTypeSerializer();
        var witnessData = Array.Empty<byte>();
        var witness = new Witness(witnessData);

        using var memoryStream = new MemoryStream();

        // When
        await witnessSerializer.SerializeAsync(witness, memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var deserializedWitness = await witnessSerializer.DeserializeAsync(memoryStream);

        // Then
        Assert.Equal(witnessData, deserializedWitness);
    }

    [Fact]
    public async Task Given_InvalidStream_When_Deserialized_Then_ThrowsSerializationException()
    {
        // Given
        var witnessSerializer = new WitnessTypeSerializer();
        using var memoryStream = new MemoryStream([0x00]);

        // When & Then
        await Assert.ThrowsAsync<SerializationException>(async () =>
            await witnessSerializer.DeserializeAsync(memoryStream));
    }
    
    [Fact]
    public async Task Given_LargeWitnessData_When_SerializedAndDeserialized_Then_DataIsPreserved()
    {
        // Given
        var witnessSerializer = new WitnessTypeSerializer();
        var witnessData = new byte[ushort.MaxValue];
        for (var i = 0; i < witnessData.Length; i++)
        {
            witnessData[i] = (byte)(i % 256);
        }
        var witness = new Witness(witnessData);

        using var memoryStream = new MemoryStream();

        // When
        await witnessSerializer.SerializeAsync(witness, memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var deserializedWitness = await witnessSerializer.DeserializeAsync(memoryStream);

        // Then
        Assert.Equal(witnessData, deserializedWitness);
    }
}