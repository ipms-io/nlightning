namespace NLightning.Domain.Protocol.Constants;

/// <summary>
/// Represents the message types used in the Lightning Network.
/// </summary>
public enum MessageTypes : ushort
{
    #region Setup & Control
    Warning = 1,
    Stfu = 2,
    Init = 16,
    Error = 17,
    Ping = 18,
    Pong = 19,
    #endregion

    #region Channel
    OpenChannel = 32,   // NOT IMPLEMENTED
    AcceptChannel = 33, // NOT IMPLEMENTED
    FundingCreated = 34, // NOT IMPLEMENTED
    FundingSigned = 35, // NOT IMPLEMENTED
    ChannelReady = 36,
    Shutdown = 38,
    ClosingSigned = 39,
    OpenChannel2 = 64,
    AcceptChannel2 = 65,
    #endregion

    #region Interactive Transaction Construction
    TxAddInput = 66,
    TxAddOutput = 67,
    TxRemoveInput = 68,
    TxRemoveOutput = 69,
    TxComplete = 70,
    TxSignatures = 71,
    TxInitRbf = 72,
    TxAckRbf = 73,
    TxAbort = 74,
    #endregion

    #region Commitment
    UpdateAddHtlc = 128,
    UpdateFulfillHtlc = 130,
    UpdateFailHtlc = 131,
    CommitmentSigned = 132,
    RevokeAndAck = 133,
    UpdateFee = 134,
    UpdateFailMalformedHtlc = 135,
    ChannelReestablish = 136,
    #endregion

    #region Routing
    AnnouncementSignatures = 259,
    ChannelAnnouncement = 256,
    NodeAnnouncement = 257,
    ChannelUpdate = 258
    #endregion
}