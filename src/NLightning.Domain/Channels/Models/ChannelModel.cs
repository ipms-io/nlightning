namespace NLightning.Domain.Channels.Models;

using Transactions.Outputs;
using Bitcoin.ValueObjects;
using Crypto.ValueObjects;
using Enums;
using Money;
using Protocol.ValueObjects;
using ValueObjects;

public class ChannelModel
{
    #region Base Properties

    public ChannelConfig ChannelConfig { get; }
    public ChannelId ChannelId { get; }
    public FundingOutputInfo FundingOutput { get; }
    public bool IsInitiator { get; }
    public CompactPubKey RemoteNodeId { get; }
    public ChannelState State { get; private set; }
    public ChannelVersion Version { get; set; }
    public CommitmentNumber CommitmentNumber { get; }

    #endregion

    #region Signatures

    public CompactSignature? LastSentSignature { get; }
    public CompactSignature? LastReceivedSignature { get; }

    #endregion

    #region Local Information

    public LightningMoney LocalBalance { get; }
    public ChannelKeySetModel LocalKeySet { get; }
    public ulong LocalNextHtlcId { get; }
    public ICollection<Htlc>? LocalOfferedHtlcs { get; }
    public ICollection<Htlc>? LocalFullfiledHtlcs { get; }
    public ICollection<Htlc>? LocalOldHtlcs { get; }
    public ulong LocalRevocationNumber { get; }
    public BitcoinScript? LocalUpfrontShutdownScript { get; }

    #endregion

    #region Remote Information

    public LightningMoney RemoteBalance { get; }
    public ChannelKeySetModel RemoteKeySet { get; }
    public ulong RemoteNextHtlcId { get; }
    public ICollection<Htlc>? RemoteOfferedHtlcs { get; }
    public ICollection<Htlc>? RemoteFulffiledHtlcs { get; }
    public ICollection<Htlc>? RemoteOldHtlcs { get; }
    public ulong RemoteRevocationNumber { get; }
    public BitcoinScript? RemoteUpfrontShutdownScript { get; }

    #endregion

    public ChannelModel(ChannelConfig channelConfig, ChannelId channelId, CommitmentNumber commitmentNumber,
                        FundingOutputInfo fundingOutput, bool isInitiator, CompactSignature? lastSentSignature,
                        CompactSignature? lastReceivedSignature, LightningMoney localBalance,
                        ChannelKeySetModel localKeySet,
                        ulong localNextHtlcId, ulong localRevocationNumber, LightningMoney remoteBalance,
                        ChannelKeySetModel remoteKeySet, ulong remoteNextHtlcId, CompactPubKey remoteNodeId,
                        ulong remoteRevocationNumber, ChannelState state, ChannelVersion version,
                        ICollection<Htlc>? localOfferedHtlcs = null, ICollection<Htlc>? localFulffiledHtlcs = null,
                        ICollection<Htlc>? localOldHtlcs = null, ICollection<Htlc>? remoteOfferedHtlcs = null,
                        ICollection<Htlc>? remoteFullfiledHtlcs = null, ICollection<Htlc>? remoteOldHtlcs = null)
    {
        ChannelConfig = channelConfig;
        ChannelId = channelId;
        CommitmentNumber = commitmentNumber;
        FundingOutput = fundingOutput;
        IsInitiator = isInitiator;
        LastSentSignature = lastSentSignature;
        LastReceivedSignature = lastReceivedSignature;
        LocalBalance = localBalance;
        LocalKeySet = localKeySet;
        LocalNextHtlcId = localNextHtlcId;
        LocalRevocationNumber = localRevocationNumber;
        RemoteBalance = remoteBalance;
        RemoteKeySet = remoteKeySet;
        RemoteNextHtlcId = remoteNextHtlcId;
        RemoteRevocationNumber = remoteRevocationNumber;
        State = state;
        Version = version;
        RemoteNodeId = remoteNodeId;
        LocalOfferedHtlcs = localOfferedHtlcs ?? new List<Htlc>();
        LocalFullfiledHtlcs = localFulffiledHtlcs ?? new List<Htlc>();
        LocalOldHtlcs = localOldHtlcs ?? new List<Htlc>();
        RemoteOfferedHtlcs = remoteOfferedHtlcs ?? new List<Htlc>();
        RemoteFulffiledHtlcs = remoteFullfiledHtlcs ?? new List<Htlc>();
        RemoteOldHtlcs = remoteOldHtlcs ?? new List<Htlc>();
    }

    public void UpdateState(ChannelState newState)
    {
        if (State == ChannelState.V2Opening && newState < ChannelState.V2Opening
         || State >= ChannelState.V1Opening && newState == ChannelState.V2Opening)
            throw new ArgumentOutOfRangeException(nameof(newState), "Invalid channel state for update.");

        if (newState <= State)
            throw new ArgumentOutOfRangeException(nameof(newState), "New state must be greater than current state.");

        State = newState;
    }
}