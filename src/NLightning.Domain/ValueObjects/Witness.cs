using System.Runtime.Serialization;
using NLightning.Domain.Serialization;

namespace NLightning.Domain.ValueObjects;

public class Witness(byte[] witnessData)
{
    private static IEndianConverter? s_endianConverter;
    private static IEndianConverter _endianConverter => 
        s_endianConverter ?? throw new InvalidOperationException("EndianConverter not initialized");
    
    public static void SetEndianConverter(IEndianConverter converter) => s_endianConverter = converter;
    
    public byte[] WitnessData { get; } = witnessData;

    public async Task SerializeAsync(Stream stream)
    {
        await stream.WriteAsync(_endianConverter.GetBytesBigEndian((ushort)WitnessData.Length));
        await stream.WriteAsync(WitnessData);
    }

    public static async Task<Witness> DeserializeAsync(Stream stream)
    {
        try
        {
            var bytes = new byte[2];
            await stream.ReadExactlyAsync(bytes);
            var length = _endianConverter.ToUInt16BigEndian(bytes);

            var witnessData = new byte[length];
            await stream.ReadExactlyAsync(witnessData);

            return new Witness(witnessData);
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing Witness", e);
        }
    }
}