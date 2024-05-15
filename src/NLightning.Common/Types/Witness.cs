using System.Runtime.Serialization;

namespace NLightning.Common.Types;

using Utils;

public class Witness(byte[] witnessData)
{
    public byte[] WitnessData { get; } = witnessData;

    public async Task SerializeAsync(Stream stream)
    {
        await stream.WriteAsync(EndianBitConverter.GetBytesBE((ushort)WitnessData.Length));
        await stream.WriteAsync(WitnessData);
    }

    public static async Task<Witness> DeserializeAsync(Stream stream)
    {
        try
        {
            var bytes = new byte[2];
            await stream.ReadExactlyAsync(bytes);
            var length = EndianBitConverter.ToUInt16BE(bytes);

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