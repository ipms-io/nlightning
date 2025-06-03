namespace NLightning.Domain.Channels.Factories;

using Bitcoin.Interfaces;
using Bitcoin.ValueObjects;
using Constants;
using Enums;
using Interfaces;
using Models;
using ValueObjects;
using Crypto.Hashes;
using Crypto.ValueObjects;
using Domain.Enums;
using Exceptions;
using Money;
using Node.Options;
using Protocol.Interfaces;
using Protocol.Messages;
using Protocol.ValueObjects;
using Transactions.Outputs;

public class ChannelFactory : IChannelFactory
{
    private readonly IChannelKeySetFactory _channelKeySetFactory;
    private readonly IFeeService _feeService;
    private readonly NodeOptions _nodeOptions;
    private readonly ISha256 _sha256;

    public ChannelFactory(IChannelKeySetFactory channelKeySetFactory, IFeeService feeService, NodeOptions nodeOptions,
                          ISha256 sha256)
    {
        _feeService = feeService;
        _nodeOptions = nodeOptions;
        _sha256 = sha256;
        _channelKeySetFactory = channelKeySetFactory;
    }

    public Channel CreateChannelV1AsNonInitiator(OpenChannel1Message message, FeatureOptions negotiatedFeatures,
                                                 CompactPubKey remoteNodeId)
    {
        var payload = message.Payload;

        #region Checks

        // Check if ChainHash is compatible
        if (payload.ChainHash != _nodeOptions.BitcoinNetwork.ChainHash)
            throw new ChannelErrorException("ChainHash is not compatible");

        // If dual fund is negotiated fail the channel
        if (negotiatedFeatures.DualFund == FeatureSupport.Compulsory)
            throw new ChannelErrorException("We can only accept dual fund channels");

        // Check if this is a large channel and if we support it
        if (payload.FundingAmount >= ChannelConstants.LargeChannelAmount &&
            negotiatedFeatures.LargeChannels == FeatureSupport.No)
            throw new ChannelErrorException("We don't support large channels");

        // Check if the push amount is too large
        if (payload.PushAmount > 1_000 * payload.FundingAmount)
            throw new ChannelErrorException("Push amount is too large");

        // Check if there are enough funds to pay for fees
        if (payload.FundingAmount < payload.DustLimitAmount + payload.ChannelReserveAmount)
            throw new ChannelErrorException("Funding amount is too small");

        // Check if dust_limit_satoshis is too small
        if (payload.DustLimitAmount < ChannelConstants.MinDustLimitAmount)
            throw new ChannelErrorException("Dust limit amount is too small");

        // Check if we consider dust_limit_satoshis too large. IE. 20% bigger than our dust limit
        if (payload.DustLimitAmount > _nodeOptions.DustLimitAmount * 1.2M)
            throw new ChannelErrorException("Dust limit amount is too large");

        // Check if the channel reserve amount is below the dust limit
        if (payload.DustLimitAmount > payload.ChannelReserveAmount)
            throw new ChannelErrorException("Channel reserve amount is too small");

        // Check if we consider channel_reserve_satoshis too large. IE. 20% bigger than our channel reserve
        if (payload.ChannelReserveAmount > _nodeOptions.ChannelReserveAmount * 1.2M)
            throw new ChannelErrorException("Channel reserve amount is too large");

        // Check channel flags for undefined bits
        // if ((payload.ChannelFlags & ~1) != 0)
        //     _logger.LogWarning("[{methodName}] ChannelId: {channelId} | Channel flags have undefined bits set",
        //                        nameof(CreateChannelV1AsNonInitiator), payload.ChannelId);

        // Check for the upfront shutdown script
        if (negotiatedFeatures.UpfrontShutdownScript == FeatureSupport.Compulsory &&
            message.UpfrontShutdownScriptTlv?.Length == 0)
            throw new ChannelErrorException("Upfront shutdown script is required but not provided");

        // Check if the option channel type was negotiated and the channel type is present
        if (negotiatedFeatures.ChannelType == FeatureSupport.Compulsory && message.ChannelTypeTlv is null)
            throw new ChannelErrorException("Channel type was negotiated but not provided");

        // Check ChannelType against negotiated options
        var minimumDepth = _nodeOptions.MinimumDepth;
        if (message.ChannelTypeTlv is not null)
        {
            // Check if it set any non-negotiated features
            if (!negotiatedFeatures.GetNodeFeatures()
                                   .IsCompatible(message.ChannelTypeTlv.Features, out var negotiatedChannelFeatures) ||
                negotiatedChannelFeatures is null)
                throw new ChannelErrorException("Negotiated channel features are not compatible with channel type");

            // Check if a channel should be announced but option scid alias is negotiated
            if (negotiatedChannelFeatures.IsFeatureSet(Feature.OptionScidAlias) && payload.ChannelFlags.AnnounceChannel)
                throw new ChannelErrorException("Invalid channel flags for OPTION_SCID_ALIAS");

            // Check for ZeroConf feature
            if (message.ChannelTypeTlv.Features.IsFeatureSet(Feature.OptionZeroconf, true))
            {
                if (_nodeOptions.Features.ZeroConf == FeatureSupport.No)
                    throw new ChannelErrorException("ZeroConf feature not supported but requested by peer");

                minimumDepth = 0U;
            }
        }

        // Check if Funding Satoshis is too small
        if (_nodeOptions.MinimumChannelSize > payload.FundingAmount)
            throw new ChannelErrorException("Funding amount is too small");

        // Check if we consider htlc_minimum_msat too large. IE. 20% bigger than our htlc minimum amount
        if (payload.HtlcMinimumAmount > _nodeOptions.HtlcMinimumAmount * 1.2M)
            throw new ChannelErrorException("Htlc minimum amount is too large");

        // Check if we consider max_htlc_value_in_flight_msat too small. IE. 20% smaller than our maximum htlc value
        var maxHtlcValueInFlight =
            LightningMoney.Satoshis(_nodeOptions.AllowUpToPercentageOfChannelFundsInFlight *
                                    payload.FundingAmount.Satoshi / 100M);
        if (payload.MaxHtlcValueInFlight < maxHtlcValueInFlight * 0.8M)
            throw new ChannelErrorException("Max htlc value in flight is too small");

        // Check if we consider max_accepted_htlcs too small. IE. 20% smaller than our max-accepted htlcs
        if (payload.MaxAcceptedHtlcs < (ushort)(_nodeOptions.MaxAcceptedHtlcs * 0.8M))
            throw new ChannelErrorException("Max accepted htlcs is too small");

        // Check max_accepted_htlcs is too large
        if (payload.MaxAcceptedHtlcs > ChannelConstants.MaxAcceptedHtlcs)
            throw new ChannelErrorException("Max accepted htlcs is too small");

        // Check if we consider to_self_delay unreasonably large. IE. 50% bigger than our to_self_delay
        if (payload.ToSelfDelay > _nodeOptions.ToSelfDelay * 1.5M)
            throw new ChannelErrorException("To self delay is too large");

        // Check if we consider fee_rate_per_kw too large
        // TODO: Get actual amount from FeeService
        if (payload.FeeRatePerKw > ChannelConstants.MaxFeePerKw)
            throw new ChannelErrorException("Fee rate per kw is too large");

        // Check if we consider fee_rate_per_kw too small. IE. 20% smaller than our fee rate
        // TODO: Get actual amount from FeeService
        if (payload.FeeRatePerKw < ChannelConstants.MinFeePerKw ||
            payload.FeeRatePerKw < _feeService.GetCachedFeeRatePerKw() * 0.8M)
            throw new ChannelErrorException("Fee rate per kw is too small");

        #endregion

        // Calculate the amounts
        var toLocalAmount = payload.PushAmount;
        var toRemoteAmount = payload.FundingAmount - payload.PushAmount;

        #region Secrets and Basepoints

        // Generate a private key for this channel
        var localKeySet = _channelKeySetFactory.CreateNew();
        var remoteKeySet = _channelKeySetFactory.CreateFromRemoteInfo(payload);

        BitcoinScript? localUpfrontShutdownScript = null;
        BitcoinScript? remoteUpfrontShutdownScript = null;
        // Generate our upfront shutdown script
        if (message.UpfrontShutdownScriptTlv is not null)
        {
            // Use the script from the message
            remoteUpfrontShutdownScript = message.UpfrontShutdownScriptTlv.Value;
        }

        if (_nodeOptions.Features.UpfrontShutdownScript > FeatureSupport.No)
        {
            // Generate our upfront shutdown script
            // TODO: Generate a script from the local key set
            // localUpfrontShutdownScript = ;
        }

        #endregion

        // Generate the channel configuration
        var channelConfig = new ChannelConfig(payload.ChannelReserveAmount, _nodeOptions.DustLimitAmount,
                                              payload.FeeRatePerKw, payload.HtlcMinimumAmount, payload.MaxAcceptedHtlcs,
                                              payload.MaxHtlcValueInFlight, minimumDepth,
                                              negotiatedFeatures.AnchorOutputs != FeatureSupport.No,
                                              payload.DustLimitAmount, payload.ToSelfDelay, localUpfrontShutdownScript,
                                              remoteUpfrontShutdownScript);

        // Generate the commitment numbers
        var localCommitmentNumber = new CommitmentNumber(localKeySet.PaymentCompactBasepoint,
                                                         remoteKeySet.PaymentCompactBasepoint, _sha256);
        var remoteCommitmentNumber = new CommitmentNumber(remoteKeySet.PaymentCompactBasepoint,
                                                          localKeySet.PaymentCompactBasepoint, _sha256);

        try
        {
            var fundingOutput = new FundingOutputInfo(payload.FundingAmount, localKeySet.PaymentCompactBasepoint,
                                                      remoteKeySet.PaymentCompactBasepoint);

            // Create the channel
            return new Channel(channelConfig, payload.ChannelId, fundingOutput, false, null, null, toLocalAmount,
                               localCommitmentNumber, localKeySet, 1, 0, toRemoteAmount, remoteCommitmentNumber,
                               remoteKeySet, 1, remoteNodeId, 0, ChannelState.V1Opening, ChannelVersion.V1);
        }
        catch (Exception e)
        {
            throw new ChannelErrorException("Error creating commitment transaction", e);
        }
    }
}