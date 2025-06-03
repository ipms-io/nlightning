using NLightning.Domain.Transactions.Outputs;

namespace NLightning.Domain.Channels.Models;

using Bitcoin.ValueObjects;
using Crypto.ValueObjects;
using Enums;
using Money;
using Protocol.ValueObjects;
using ValueObjects;

public class Channel
{
    #region Base Properties
    public ChannelConfig ChannelConfig { get; }
    public ChannelId ChannelId { get; }
    public FundingOutputInfo FundingOutput { get; }
    public bool IsInitiator { get; }
    public CompactPubKey RemoteNodeId { get; }
    public ChannelState State { get; }
    public ChannelVersion Version { get; set; }
    #endregion
    
    #region Signatures
    public DerSignature? LastSentSignature { get; }
    public DerSignature? LastReceivedSignature { get; }
    #endregion
    
    #region Local Information
    public LightningMoney LocalBalance { get; }
    public CommitmentNumber LocalCommitmentNumber { get; }
    public ChannelKeySet LocalKeySet { get; }
    public ulong LocalNextHtlcId { get; }
    public ICollection<Htlc>? LocalOfferedHtlcs { get; }
    public ICollection<Htlc>? LocalFulffiledHtlcs { get; }
    public ICollection<Htlc>? LocalOldHtlcs { get; }
    public ulong LocalRevocationNumber { get; }
    public BitcoinScript? LocalUpfrontShutdownScript { get; }
    #endregion
    
    #region Remote Information
    public LightningMoney RemoteBalance { get; }
    public CommitmentNumber RemoteCommitmentNumber { get; }
    public ChannelKeySet RemoteKeySet { get; }
    public ulong RemoteNextHtlcId { get; }
    public ICollection<Htlc>? RemoteOfferedHtlcs { get; }
    public ICollection<Htlc>? RemoteFulffiledHtlcs { get; }
    public ICollection<Htlc>? RemoteOldHtlcs { get; }
    public ulong RemoteRevocationNumber { get; }
    public BitcoinScript? RemoteUpfrontShutdownScript { get; }
    #endregion
    
    public Channel(ChannelConfig channelConfig, ChannelId channelId, FundingOutputInfo fundingOutput, bool isInitiator,
                   DerSignature? lastSentSignature, DerSignature? lastReceivedSignature, LightningMoney localBalance,
                   CommitmentNumber localCommitmentNumber, ChannelKeySet localKeySet, ulong localNextHtlcId,
                   ulong localRevocationNumber, LightningMoney remoteBalance,
                   CommitmentNumber remoteCommitmentNumber, ChannelKeySet remoteKeySet, ulong remoteNextHtlcId,
                   CompactPubKey remoteNodeId, ulong remoteRevocationNumber, ChannelState state, ChannelVersion version,
                   ICollection<Htlc>? localOfferedHtlcs = null, ICollection<Htlc>? localFulffiledHtlcs = null,
                   ICollection<Htlc>? localOldHtlcs = null, ICollection<Htlc>? remoteOfferedHtlcs = null,
                   ICollection<Htlc>? remoteFulffiledHtlcs = null, ICollection<Htlc>? remoteOldHtlcs = null)
    {
        ChannelConfig = channelConfig;
        ChannelId = channelId;
        FundingOutput = fundingOutput;
        IsInitiator = isInitiator;
        LastSentSignature = lastSentSignature;
        LastReceivedSignature = lastReceivedSignature;
        LocalBalance = localBalance;
        LocalCommitmentNumber = localCommitmentNumber;
        LocalKeySet = localKeySet;
        LocalNextHtlcId = localNextHtlcId;
        LocalRevocationNumber = localRevocationNumber;
        RemoteBalance = remoteBalance;
        RemoteCommitmentNumber = remoteCommitmentNumber;
        RemoteKeySet = remoteKeySet;
        RemoteNextHtlcId = remoteNextHtlcId;
        RemoteRevocationNumber = remoteRevocationNumber;
        State = state;
        Version = version;
        RemoteNodeId = remoteNodeId;
        LocalOfferedHtlcs = localOfferedHtlcs ?? new List<Htlc>();
        LocalFulffiledHtlcs = localFulffiledHtlcs ?? new List<Htlc>();
        LocalOldHtlcs = localOldHtlcs ?? new List<Htlc>();
        RemoteOfferedHtlcs = remoteOfferedHtlcs ?? new List<Htlc>();
        RemoteFulffiledHtlcs = remoteFulffiledHtlcs ?? new List<Htlc>();
        RemoteOldHtlcs = remoteOldHtlcs ?? new List<Htlc>();
    }
}