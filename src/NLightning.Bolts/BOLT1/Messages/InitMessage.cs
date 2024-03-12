using NLightning.Bolts.BOLT1.Interfaces;
using NLightning.Bolts.BOLT1.Types;

namespace NLightning.Bolts.BOLT1.Messages;

public class InitMessage(byte[] globalFeatures, byte[] localFeatures) : BaseMessage
{
    public static new byte Type => 16;

    public InitData Data { get; } = new InitData(globalFeatures, localFeatures);

    public TLVStream? TLVS { get; set; }

    public Dictionary<byte, IInitType<object>>? Types { get; set; }

    public override byte[] Serialize()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write(Type);
        writer.Write(Data.GlobalFeaturesLength);
        writer.Write(Data.GlobalFeatures);
        writer.Write(Data.LocalFeaturesLength);
        writer.Write(Data.LocalFeatures);

        if (TLVS != null)
        {
            writer.Write(TLVS.Serialize());
        }

        if (Types != null)
        {
            foreach (var type in Types)
            {
                writer.Write(type.Value.Serialize());
            }
        }

        return stream.ToArray();
    }
}
