using MessagePack;

namespace NLightning.Transport.Ipc;

/// <summary>
/// Error payload
/// </summary>
[MessagePackObject]
public sealed class IpcError
{
    [Key(0)] public string Code { get; init; } = string.Empty;
    [Key(1)] public string Message { get; init; } = string.Empty;
}