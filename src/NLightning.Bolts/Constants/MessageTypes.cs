namespace NLightning.Bolts.Constants;

/// <summary>
/// Represents the message types used in the Lightning Network.
/// </summary>
public static class MessageTypes
{
    #region Setup & Control
    public const ushort WARNING = 1,
                        INIT = 16,
                        ERROR = 17,
                        PING = 18,
                        PONG = 19;
    #endregion

    #region Channel
    public const ushort OPEN_CHANNEL = 32,
                        ACCEPT_CHANNEL = 33,
                        FUNDING_CREATED = 34,
                        FUNDING_SIGNED = 35,
                        FUNDING_LOCKED = 36,
                        SHUTDOWN = 38,
                        CLOSING_SIGNED = 39;
    #endregion

    #region Interactive Transaction Construction
    public const ushort TX_ADD_INPUT = 66,
                        TX_ADD_OUTPUT = 67,
                        TX_REMOVE_INPUT = 68,
                        TX_REMOVE_OUTPUT = 69,
                        TX_COMPLETE = 70,
                        TX_SIGNATURES = 71,
                        TX_INIT_RBF = 72;
    #endregion

    #region Commitment
    public const ushort UPDATE_ADD_HTLC = 128,
                        UPDATE_FULFILL_HTLC = 130,
                        UPDATE_FAIL_HTLC = 131,
                        COMMITMENT_SIGNED = 132,
                        REVOKE_AND_ACK = 133,
                        UPDATE_FEE = 134,
                        CHANNEL_REESTABLISH = 136;
    #endregion

    #region Routing
    public const ushort ANNOUNCEMENT_SIGNATURES = 259,
                        CHANNEL_ANNOUNCEMENT = 256,
                        NODE_ANNOUNCEMENT = 257,
                        CHANNEL_UPDATE = 258;
    #endregion
}