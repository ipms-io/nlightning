namespace NLightning.Bolts.BOLT11.Enums;

public enum TaggedFieldTypes : byte
{
    PaymentHash = 1,
    RoutingInfo = 3,
    Features = 5,
    ExpiryTime = 6,
    FallbackAddress = 9,
    Description = 13,
    PaymentSecret = 16,
    PayeePubKey = 19,
    DescriptionHash = 23,
    MinFinalCltvExpiry = 24,
    Metadata = 27
}