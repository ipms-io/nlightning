using System.Text.Json.Serialization;

namespace NLightning.Infrastructure.Node.Models;

public class KeyFileData
{
    [JsonPropertyName("network")] public string Network { get; set; } = string.Empty;

    [JsonPropertyName("descriptor")] public string Descriptor { get; set; } = string.Empty;

    [JsonPropertyName("lastUsedIndex")] public uint LastUsedIndex { get; set; }

    [JsonPropertyName("encryptedExtKey")] public string EncryptedExtKey { get; set; } = string.Empty;
}