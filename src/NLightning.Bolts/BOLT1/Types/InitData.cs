namespace NLightning.Bolts.BOLT1.Types;

public class InitData(byte[] globalFeatures, byte[] localFeatures)
{
    public U16 GlobalFeaturesLength => (byte)GlobalFeatures.Length;
    public byte[] GlobalFeatures { get; } = globalFeatures;
    public U16 LocalFeaturesLength => (byte)LocalFeatures.Length;
    public byte[] LocalFeatures { get; } = localFeatures;
    public int TLVS { get; set; }
}