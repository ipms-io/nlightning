using NLightning.Common.Utils;

namespace NLightning.Bolt11.Tests.Mocks;

using Bolt11.Enums;
using Bolt11.Interfaces;

public class MockTaggedField : ITaggedField
{
    public TaggedFieldTypes Type { get; init; }
    public short Length { get; init; }
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