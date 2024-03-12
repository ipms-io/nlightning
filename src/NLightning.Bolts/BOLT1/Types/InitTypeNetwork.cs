using NLightning.Bolts.BOLT1.Interfaces;

namespace NLightning.Bolts.BOLT1.Types;

public class InitTypeNetwork(byte type, List<ChainHash> data) : IInitType<List<ChainHash>>
{
    public byte Type { get; set; } = type;
    public List<ChainHash> Data { get; set; } = data ?? throw new ArgumentNullException(nameof(data));

    public void Deserialize(byte[] data)
    {
        // serialize the type
        Type = data[0];
        // check if data is sound
        if (data.Length % 32 != 1)
        {
            throw new ArgumentException("Invalid data length");
        }
        // deserialize the data
        for (var i = 1; i < data.Length; i += 32)
        {
            Data.Add(new ChainHash(data[i..(i + 32)]));
        }
    }

    public byte[] Serialize()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write(Type);

        foreach (var chainHash in Data)
        {
            writer.Write(chainHash);
        }

        return stream.ToArray();
    }
}