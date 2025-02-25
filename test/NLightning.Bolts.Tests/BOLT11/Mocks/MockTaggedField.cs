namespace NLightning.Bolts.Tests.BOLT11.Mocks;

using Bolts.BOLT11.Enums;
using Bolts.BOLT11.Interfaces;
using Common.BitUtils;

public class MockTaggedField : ITaggedField
{
    public TaggedFieldTypes Type { get; set; }
    public short Length { get; set; }
    public bool Valid { get; set; } = true;  // controls IsValid()

    public bool IsValid() => Valid;

    public void WriteToBitWriter(BitWriter bitWriter)
    {
        // For test purposes, we can do something trivial
        // e.g. write length "Length" times
        for (var i = 0; i < Length; i++)
        {
            bitWriter.WriteByteAsBits(0xFF, 1);
        }
    }
}