namespace NLightning.Bolt11.Enums;

/// <summary>
/// Types used for the tagged fields
/// </summary>
public enum TaggedFieldTypes : byte
{
    /// <summary>
    /// The payment hash
    /// </summary>
    /// <remarks>
    /// represented by the letter p
    /// </remarks>
    PAYMENT_HASH = 1,

    /// <summary>
    /// The Routing Information
    /// </summary>
    /// <remarks>
    /// represented by the letter r
    /// </remarks>
    ROUTING_INFO = 3,

    /// <summary>
    /// The Features
    /// </summary>
    /// <remarks>
    /// represented by the letter 9
    /// </remarks>
    FEATURES = 5,

    /// <summary>
    /// The Expiry Time
    /// </summary>
    /// <remarks>
    /// represented by the letter x
    /// </remarks>
    EXPIRY_TIME = 6,

    /// <summary>
    /// The FallBack Address
    /// </summary>
    /// <remarks>
    /// represented by the letter f
    /// </remarks>
    FALLBACK_ADDRESS = 9,

    /// <summary>
    /// The Description
    /// </summary>
    /// <remarks>
    /// represented by the letter d
    /// </remarks>
    DESCRIPTION = 13,

    /// <summary>
    /// The Payment Secret
    /// </summary>
    /// <remarks>
    /// represented by the letter s
    /// </remarks>
    PAYMENT_SECRET = 16,

    /// <summary>
    /// The Payee Public Key
    /// </summary>
    /// <remarks>
    /// represented by the letter n
    /// </remarks>
    PAYEE_PUB_KEY = 19,

    /// <summary>
    /// The Description Hash
    /// </summary>
    /// <remarks>
    /// represented by the letter h
    /// </remarks>
    DESCRIPTION_HASH = 23,

    /// <summary>
    /// The Min Final Cltv Expiry
    /// </summary>
    /// <remarks>
    /// represented by the letter c
    /// </remarks>
    MIN_FINAL_CLTV_EXPIRY = 24,

    /// <summary>
    /// The Additional Metadata
    /// </summary>
    /// <remarks>
    /// represented by the letter m
    /// </remarks>
    METADATA = 27
}