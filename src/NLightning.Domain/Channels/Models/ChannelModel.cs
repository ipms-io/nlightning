using NLightning.Domain.Protocol.Models;

namespace NLightning.Domain.Channels.Models;

using Bitcoin.Transactions.Outputs;
using Bitcoin.ValueObjects;
using Crypto.ValueObjects;
using Enums;
using Money;
using ValueObjects;

public class ChannelModel
{
    #region Base Properties

    public ChannelConfig ChannelConfig { get; }
    public ChannelId ChannelId { get; private set; }
    public ShortChannelId ShortChannelId { get; set; }
    public CommitmentNumber CommitmentNumber { get; }
    public uint FundingCreatedAtBlockHeight { get; set; }
    public FundingOutputInfo FundingOutput { get; }
    public bool IsInitiator { get; }
    public CompactPubKey RemoteNodeId { get; }
    public ChannelState State { get; private set; }
    public ChannelVersion Version { get; }

    #endregion

    #region Signatures

    public CompactSignature? LastSentSignature { get; }
    public CompactSignature? LastReceivedSignature { get; }

    #endregion

    #region Local Information

    public ICollection<ShortChannelId>? LocalAliases { get; set; }
    public LightningMoney LocalBalance { get; }
    public ChannelKeySetModel LocalKeySet { get; }
    public ulong LocalNextHtlcId { get; }
    public ICollection<Htlc>? LocalOfferedHtlcs { get; }
    public ICollection<Htlc>? LocalFulfilledHtlcs { get; }
    public ICollection<Htlc>? LocalOldHtlcs { get; }
    public ulong LocalRevocationNumber { get; }
    public BitcoinScript? LocalUpfrontShutdownScript { get; }

    #endregion

    #region Remote Information

    public ShortChannelId? RemoteAlias { get; set; }
    public LightningMoney RemoteBalance { get; }
    public ChannelKeySetModel RemoteKeySet { get; }
    public ulong RemoteNextHtlcId { get; }
    public ulong RemoteRevocationNumber { get; }
    public ICollection<Htlc>? RemoteFulfilledHtlcs { get; }
    public ICollection<Htlc>? RemoteOfferedHtlcs { get; }
    public ICollection<Htlc>? RemoteOldHtlcs { get; }
    public BitcoinScript? RemoteUpfrontShutdownScript { get; }

    #endregion

    public ChannelModel(ChannelConfig channelConfig, ChannelId channelId, CommitmentNumber commitmentNumber,
                        FundingOutputInfo fundingOutput, bool isInitiator, CompactSignature? lastSentSignature,
                        CompactSignature? lastReceivedSignature, LightningMoney localBalance,
                        ChannelKeySetModel localKeySet, ulong localNextHtlcId, ulong localRevocationNumber,
                        LightningMoney remoteBalance, ChannelKeySetModel remoteKeySet, ulong remoteNextHtlcId,
                        CompactPubKey remoteNodeId, ulong remoteRevocationNumber, ChannelState state,
                        ChannelVersion version, ICollection<Htlc>? localOfferedHtlcs = null,
                        ICollection<Htlc>? localFulfilledHtlcs = null, ICollection<Htlc>? localOldHtlcs = null,
                        BitcoinScript? localUpfrontShutdownScript = null, ICollection<Htlc>? remoteOfferedHtlcs = null,
                        ICollection<Htlc>? remoteFulfilledHtlcs = null, ICollection<Htlc>? remoteOldHtlcs = null,
                        BitcoinScript? remoteUpfrontShutdownScript = null)
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
        LocalFulfilledHtlcs = localFulfilledHtlcs ?? new List<Htlc>();
        LocalOldHtlcs = localOldHtlcs ?? new List<Htlc>();
        RemoteOfferedHtlcs = remoteOfferedHtlcs ?? new List<Htlc>();
        RemoteFulfilledHtlcs = remoteFulfilledHtlcs ?? new List<Htlc>();
        RemoteOldHtlcs = remoteOldHtlcs ?? new List<Htlc>();
        LocalUpfrontShutdownScript = localUpfrontShutdownScript;
        RemoteUpfrontShutdownScript = remoteUpfrontShutdownScript;
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

    public void UpdateChannelId(ChannelId newChannelId)
    {
        if (newChannelId == ChannelId.Zero)
            throw new ArgumentException("New channel ID cannot be empty.", nameof(newChannelId));

        ChannelId = newChannelId;
    }

    public ChannelSigningInfo GetSigningInfo()
    {
        return new ChannelSigningInfo(FundingOutput.TransactionId!.Value, FundingOutput.Index!.Value,
                                      FundingOutput.Amount, LocalKeySet.FundingCompactPubKey,
                                      RemoteKeySet.FundingCompactPubKey, LocalKeySet.KeyIndex);
    }
}