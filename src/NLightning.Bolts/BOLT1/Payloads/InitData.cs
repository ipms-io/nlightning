namespace NLightning.Bolts.BOLT1.Types;

using Interfaces;

public class InitData(byte[] globalFeatures, byte[] localFeatures) : IMessagePayload
{
    public U16 GlobalFeaturesLength => (byte)GlobalFeatures.Length;
    public byte[] GlobalFeatures { get; } = globalFeatures;
    public U16 LocalFeaturesLength => (byte)LocalFeatures.Length;
    public byte[] LocalFeatures { get; } = localFeatures;
    public int TLVS { get; set; }

    public void Deserialize(BinaryReader data)
    {
        throw new NotImplementedException();
    }

    public byte[] Serialize()
    {
        throw new NotImplementedException();
    }
}