using MessagePack;

namespace NLightning.Node.Models;

[MessagePackObject]
public class FeeRateCacheData
{
    [Key(0)]
    public ulong FeeRate { get; set; }

    [Key(1)]
    public DateTime LastFetchTime { get; set; }
}