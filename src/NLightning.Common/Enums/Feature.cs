namespace NLightning.Common.Enums;

/// <summary>
/// The features that can be set on a node as defined on https://github.com/lightning/bolts/blob/master/09-features.md
/// </summary>
/// <remarks>
/// The values here represents the ODD (OPTIONAL) bit position in the feature flags.
/// </remarks>
public enum Feature
{
    /// <summary>
    /// 0 is for the compulsory bit, 1 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports the data_loss_protect field in the channel_reestablish message.
    /// </remarks>
    OPTION_DATA_LOSS_PROTECT = 1,

    /// <summary>
    /// 3 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports the initial_routing_sync field in the channel_reestablish message.
    /// </remarks>
    INITIAL_ROUTING_SYNC = 3,

    /// <summary>
    /// 4 is for the compulsory bit, 5 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports the upfront_shutdown_script field in the open_channel message.
    /// </remarks>
    OPTION_UPFRONT_SHUTDOWN_SCRIPT = 5,

    /// <summary>
    /// 6 is for the compulsory bit, 7 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node requires the more sophisticated gossip queries.
    /// </remarks>
    GOSSIP_QUERIES = 7,

    /// <summary>
    /// 8 is for the compulsory bit, 9 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is compulsory and is used to indicate that the node supports variable length onion messages.
    /// </remarks>
    VAR_ONION_OPTIN = 9,

    /// <summary>
    /// 10 is for the compulsory bit, 11 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports inclusion of additional information in the gossip queries.
    /// </remarks>
    GOSSIP_QUERIES_EX = 11,

    /// <summary>
    /// 12 is for the compulsory bit, 13 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports static_remotekey.
    /// </remarks>
    OPTION_STATIC_REMOTE_KEY = 13,

    /// <summary>
    /// 14 is for the compulsory bit, 15 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports payment_secret.
    /// </remarks>
    PAYMENT_SECRET = 15,

    /// <summary>
    /// 16 is for the compulsory bit, 17 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports basic multi part payments.
    /// </remarks>
    BASIC_MPP = 17,

    /// <summary>
    /// 18 is for the compulsory bit, 19 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports large channels.
    /// </remarks>
    OPTION_SUPPORT_LARGE_CHANNEL = 19,

    /// <summary>
    /// 20 is for the compulsory bit, 21 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports anchor outputs.
    /// </remarks>
    OPTION_ANCHOR_OUTPUTS = 21,

    /// <summary>
    /// 22 is for the compulsory bit, 23 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports anchor outputs with zero fee htlc transactions.
    /// </remarks>
    OPTION_ANCHORS_ZERO_FEE_HTLC_TX = 23,

    /// <summary>
    /// 24 is for the compulsory bit, 25 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports blinded paths.
    /// </remarks>
    OPTION_ROUTE_BLINDING = 25,

    /// <summary>
    /// 26 is for the compulsory bit, 27 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports future versions of segwit in shutdown.
    /// </remarks>
    OPTION_SHUTDOWN_ANY_SEGWIT = 27,

    /// <summary>
    /// 28 is for the compulsory bit, 29 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports dual funded channels (v2).
    /// </remarks>
    OPTION_DUAL_FUND = 29,

    /// <summary>
    /// 38 is for the compulsory bit, 39 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports onion messages.
    /// </remarks>
    OPTION_ONION_MESSAGES = 39,

    /// <summary>
    /// 44 is for the compulsory bit, 45 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports channel type.
    /// </remarks>
    OPTION_CHANNEL_TYPE = 45,

    /// <summary>
    /// 46 is for the compulsory bit, 47 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports channel aliases for routing.
    /// </remarks>
    OPTION_SCID_ALIAS = 47,

    /// <summary>
    /// 48 is for the compulsory bit, 49 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports payment metadata in TLV records.
    /// </remarks>
    OPTION_PAYMENT_METADATA = 49,

    /// <summary>
    /// 50 is for the compulsory bit, 51 is for the optional bit.
    /// </summary>
    /// <remarks>
    /// This feature is optional and is used to indicate that the node supports zeroconf channels.
    /// </remarks>
    OPTION_ZEROCONF = 51
}