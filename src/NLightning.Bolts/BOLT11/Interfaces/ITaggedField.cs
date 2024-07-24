namespace NLightning.Bolts.BOLT11.Interfaces;

using Enums;

public interface ITaggedField
{
    TaggedFieldTypes Type { get; }
    int Length { get; }
    byte[] Data { get; }
    bool IsValid();
    object GetValue();
}