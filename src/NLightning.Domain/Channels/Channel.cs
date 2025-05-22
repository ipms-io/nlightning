using NBitcoin;

namespace NLightning.Domain.Channels;

using Bitcoin.Outputs;
using Bitcoin.Transactions;
using Enums;
using Node.Options;
using Protocol.Managers;
using Protocol.Services;
using ValueObjects;

public class Channel
{
    private readonly uint _keyIndex;
    private readonly ISecretStorageService _secretStorageService;
    private readonly ISecureKeyManager _secureKeyManager;

    public ChannelId ChannelId { get; }
    public NodeOptions ChannelOptions { get; }
    public bool IsInitiator { get; }
    public uint MinimumDepth { get; }
    public PubKey LocalDelayedPaymentBasepoint { get; }
    public PubKey LocalFirstPerCommitmentPoint { get; }
    public PubKey LocalFundingPubKey { get; }
    public PubKey LocalHtlcBasepoint { get; }
    public PubKey LocalPaymentBasepoint { get; }
    public PubKey LocalRevocationBasepoint { get; }
    public Script? LocalUpfrontShutdownScript { get; }
    public PubKey PeerFirstPerCommitmentPoint { get; set; }
    public PubKey PeerFundingPubKey { get; }
    public Script? PeerShutdownScriptPubKey { get; set; }
    public ShortChannelId? ShortChannelId { get; private set; }
    public ChannelState State { get; private set; }
    public IFundingOutput FundingOutput { get; }

    public ITransaction? FundingTransaction { get; }
    public ICommitmentTransaction? CommitmentTransaction { get; }

    public Channel(ChannelId channelId, PubKey firstPerCommitmentPoint, bool isInitiator, uint keyIndex,
                   uint minimumDepth, PubKey peerFundingPubKey, Script? peerShutdownScriptPubKey,
                   ISecretStorageService secretStorageService, ISecureKeyManager secureKeyManager,
                   NodeOptions nodeOptions, ICommitmentTransaction commitmentTransaction,
                   PubKey localDelayedPaymentBasepoint, PubKey localFirstPerCommitmentPoint, PubKey localFundingPubKey,
                   PubKey localHtlcBasepoint, PubKey localPaymentBasepoint, PubKey localRevocationBasepoint,
                   Script? localUpfrontShutdownScript, IFundingOutput fundingOutput)
    {
        _keyIndex = keyIndex;
        _secretStorageService = secretStorageService;
        _secureKeyManager = secureKeyManager;

        ChannelId = channelId;
        ChannelOptions = nodeOptions;
        IsInitiator = isInitiator;
        MinimumDepth = minimumDepth;
        LocalDelayedPaymentBasepoint = localDelayedPaymentBasepoint;
        LocalFirstPerCommitmentPoint = localFirstPerCommitmentPoint;
        LocalFundingPubKey = localFundingPubKey;
        LocalHtlcBasepoint = localHtlcBasepoint;
        LocalPaymentBasepoint = localPaymentBasepoint;
        LocalRevocationBasepoint = localRevocationBasepoint;
        LocalUpfrontShutdownScript = localUpfrontShutdownScript;
        PeerFirstPerCommitmentPoint = firstPerCommitmentPoint;
        PeerFundingPubKey = peerFundingPubKey;
        PeerShutdownScriptPubKey = peerShutdownScriptPubKey;

        FundingOutput = fundingOutput;

        CommitmentTransaction = commitmentTransaction;

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