namespace NLightning.Domain.Enums;

[Flags]
public enum ChannelFlag : byte
{
    None = 0,
    AnnounceChannel = 1 << 0 // Bit 0
}