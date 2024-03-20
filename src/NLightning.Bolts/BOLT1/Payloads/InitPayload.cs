namespace NLightning.Bolts.BOLT1.Types;

using System.IO;
using Interfaces;

public class InitPayload(byte[] globalFeatures, byte[] features) : IMessagePayload
{
    public byte[] GlobalFeatures { get; private set; } = globalFeatures;
    public byte[] Features { get; private set; } = features;

    public void ToWriter(BinaryWriter writer)
    {
        writer.Write(GlobalFeatures.Length);
        writer.Write(GlobalFeatures);
        writer.Write(Features.Length);
        writer.Write(Features);
    }

    public static InitPayload Deserialize(BinaryReader reader)
    {
        var globalFeaturesLength = reader.ReadInt32();
        var globalFeatures = reader.ReadBytes(globalFeaturesLength);
        var featuresLength = reader.ReadInt32();
        var features = reader.ReadBytes(featuresLength);

        return new InitPayload(globalFeatures, features);
    }
}