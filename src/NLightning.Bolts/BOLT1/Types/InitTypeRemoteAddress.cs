using NLightning.Bolts.BOLT1.Interfaces;

namespace NLightning.Bolts.BOLT1.Types;

public class InitTypeRemoteAddress(byte type, byte[] data) : IInitType<byte[]>
{
    public byte Type { get; set; } = type;
    public byte[] Data { get; set; } = data ?? throw new ArgumentNullException(nameof(data));

    public void Deserialize(byte[] data)
    {
        Type = data[0];
        Data = data[1..];
    }

    public byte[] Serialize()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write(Type);
        writer.Write(Data);

        return stream.ToArray();
    }
}