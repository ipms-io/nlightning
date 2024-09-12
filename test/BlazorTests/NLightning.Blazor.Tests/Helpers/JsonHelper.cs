using System.Text.Json;

namespace NLightning.Blazor.Tests.Helpers;

internal static class JsonHelper
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    internal static T? Deserialize<T>(string jsonString)
    {
        return JsonSerializer.Deserialize<T>(jsonString, s_jsonSerializerOptions);
    }
}