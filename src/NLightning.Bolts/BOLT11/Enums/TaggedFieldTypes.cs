namespace NLightning.Bolts.BOLT11.Enums;

public enum TaggedFieldTypes : byte
{
    /// <summary>
    /// The payment hash
    /// </summary>
    /// <remarks>
    /// represented by the letter p
    /// </remarks>
    PaymentHash = 1,

    /// <summary>
    /// The Routing Information
    /// </summary>
    /// <remarks>
    /// represented by the letter r
    /// </remarks>
    RoutingInfo = 3,

    /// <summary>
    /// The Features
    /// </summary>
    /// <remarks>
    /// represented by the letter 9
    /// </remarks>
    Features = 5,

    /// <summary>
    /// The Expiry Time
    /// </summary>
    /// <remarks>
    /// represented by the letter x
    /// </remarks>
    ExpiryTime = 6,

    /// <summary>
    /// The FallBack Address
    /// </summary>
    /// <remarks>
    /// represented by the letter f
    /// </remarks>
    FallbackAddress = 9,

    /// <summary>
    /// The Description
    /// </summary>
    /// <remarks>
    /// represented by the letter d
    /// </remarks>
    Description = 13,

    /// <summary>
    /// The Payment Secret
    /// </summary>
    /// <remarks>
    /// represented by the letter s
    /// </remarks>
    PaymentSecret = 16,

    /// <summary>
    /// The Payee Public Key
    /// </summary>
    /// <remarks>
    /// represented by the letter n
    /// </remarks>
    PayeePubKey = 19,

    /// <summary>
    /// The Description Hash
    /// </summary>
    /// <remarks>
    /// represented by the letter h
    /// </remarks>
    DescriptionHash = 23,

    /// <summary>
    /// The Min Final Cltv Expiry
    /// </summary>
    /// <remarks>
    /// represented by the letter c
    /// </remarks>
    MinFinalCltvExpiry = 24,

    /// <summary>
    /// The Additional Metadata
    /// </summary>
    /// <remarks>
    /// represented by the letter m
    /// </remarks>
    Metadata = 27
}