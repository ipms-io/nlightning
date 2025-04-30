using NBitcoin;
using NLightning.Common.Interfaces;
using NLightning.Domain.Bitcoin.Transactions;
using ISecretStorageService = NLightning.Domain.Protocol.Services.ISecretStorageService;

namespace NLightning.Domain.Channels;

using Enums;
using ValueObjects;

public class Channel
{
    private readonly ISecretStorageService _secretStorageService;

    public ChannelId ChannelId { get; }
    public PubKey FirstPerCommitmentPoint { get; set; }
    public bool IsInitiator { get; }
    public uint MinimumDepth { get; set; }
    public PubKey PeerFundingPubKey { get; set; }
    public Script? PeerShutdownScriptPubKey { get; set; }
    public ShortChannelId? ShortChannelId { get; private set; }
    public ChannelState State { get; private set; }

    public ITransaction? FundingTransaction { get; set; }
    public ITransaction? CommitmentTransaction { get; set; }

    public Channel(ChannelId channelId, PubKey firstPerCommitmentPoint, bool isInitiator, uint minimumDepth, PubKey peerFundingPubKey,
                   Script? peerShutdownScriptPubKey, ISecretStorageService secretStorageService)
    {
        _secretStorageService = secretStorageService;

        ChannelId = channelId;
        FirstPerCommitmentPoint = firstPerCommitmentPoint;
        IsInitiator = isInitiator;
        State = ChannelState.V1Opening; // Initial state
        MinimumDepth = minimumDepth;
        PeerFundingPubKey = peerFundingPubKey;
        PeerShutdownScriptPubKey = peerShutdownScriptPubKey;

        State = ChannelState.None; // Initial state
    }

    public void AssignShortChannelId(ShortChannelId shortChannelId)
    {
        ShortChannelId = shortChannelId;
        // TODO: Persist the assignment to the database
    }

    public void UpdateState(ChannelState newState)
    {
        // TODO: Persist state change to the database
        State = newState;

        // TODO: Notify other components about the state change
    }
}