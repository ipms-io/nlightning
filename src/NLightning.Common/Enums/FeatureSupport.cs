namespace NLightning.Common.Enums;

/// <summary>
/// Represents the level of support for a Lightning Network feature
/// </summary>
public enum FeatureSupport : byte
{
    /// <summary>
    /// Feature is not supported
    /// </summary>
    NO = 0,

    /// <summary>
    /// Feature is supported but optional
    /// </summary>
    OPTIONAL = 1,

    /// <summary>
    /// Feature is supported and required
    /// </summary>
    COMPULSORY = 2
}