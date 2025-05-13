namespace NLightning.Domain.Enums;

/// <summary>
/// Represents the level of support for a Lightning Network feature
/// </summary>
public enum FeatureSupport : byte
{
    /// <summary>
    /// Feature is not supported
    /// </summary>
    No = 0,

    /// <summary>
    /// Feature is supported but optional
    /// </summary>
    Optional = 1,

    /// <summary>
    /// Feature is supported and required
    /// </summary>
    Compulsory = 2
}