using NLightning.Domain.Bitcoin.Transactions.Constants;
using NLightning.Domain.Bitcoin.Transactions.Outputs;

namespace NLightning.Domain.Channels.Factories;

using Bitcoin.Interfaces;
using Bitcoin.ValueObjects;
using Constants;
using Crypto.Hashes;
using Crypto.ValueObjects;
using Domain.Enums;
using Enums;
using Exceptions;
using Interfaces;
using Models;
using Money;
using Node.Options;
using Protocol.Messages;
using Protocol.Payloads;
using Protocol.Tlv;
using Protocol.ValueObjects;
using ValueObjects;

public class ChannelFactory : IChannelFactory
{
    private readonly IFeeService _feeService;
    private readonly ILightningSigner _lightningSigner;
    private readonly NodeOptions _nodeOptions;
    private readonly ISha256 _sha256;

    public ChannelFactory(IFeeService feeService, ILightningSigner lightningSigner, NodeOptions nodeOptions,
                          ISha256 sha256)
    {
        _feeService = feeService;
        _lightningSigner = lightningSigner;
        _nodeOptions = nodeOptions;
        _sha256 = sha256;
    }

    public async Task<ChannelModel> CreateChannelV1AsNonInitiatorAsync(OpenChannel1Message message,
                                                                       FeatureOptions negotiatedFeatures,
                                                                       CompactPubKey remoteNodeId)
    {
        var payload = message.Payload;

        // If dual fund is negotiated fail the channel
        if (negotiatedFeatures.DualFund == FeatureSupport.Compulsory)
            throw new ChannelErrorException("We can only accept dual fund channels");

        // Check if the channel type was negotiated and the channel type is present
        if (message.ChannelTypeTlv is not null && negotiatedFeatures.ChannelType == FeatureSupport.Compulsory)
            throw new ChannelErrorException("Channel type was negotiated but not provided");

        // Perform optional checks for the channel
        PerformOptionalChecks(payload);

        // Perform mandatory checks for the channel
        var currentFee = await _feeService.GetFeeRatePerKwAsync();
        PerformMandatoryChecks(message.ChannelTypeTlv, currentFee, negotiatedFeatures, payload, out var minimumDepth);

        // Check for the upfront shutdown script
        if (message.UpfrontShutdownScriptTlv is null
         && (negotiatedFeatures.UpfrontShutdownScript > FeatureSupport.No || message.ChannelTypeTlv is not null))
            throw new ChannelErrorException("Upfront shutdown script is required but not provided");

        BitcoinScript? remoteUpfrontShutdownScript = null;
        if (message.UpfrontShutdownScriptTlv is not null && message.UpfrontShutdownScriptTlv.Value.Length > 0)
            remoteUpfrontShutdownScript = message.UpfrontShutdownScriptTlv.Value;

        // Calculate the amounts
        var toLocalAmount = payload.PushAmount;
        var toRemoteAmount = payload.FundingAmount - payload.PushAmount;

        // Generate local keys through the signer
        var localKeyIndex = _lightningSigner.CreateNewChannel(out var localBasepoints, out var firstPerCommitmentPoint);

        // Create the local key set
        var localKeySet = new ChannelKeySetModel(localKeyIndex, localBasepoints.FundingPubKey,
                                                 localBasepoints.RevocationBasepoint, localBasepoints.PaymentBasepoint,
                                                 localBasepoints.DelayedPaymentBasepoint, localBasepoints.HtlcBasepoint,
                                                 firstPerCommitmentPoint);

        // Create the remote key set from the message
        var remoteKeySet = ChannelKeySetModel.CreateForRemote(message.Payload.FundingPubKey,
                                                              message.Payload.RevocationBasepoint,
                                                              message.Payload.PaymentBasepoint,
                                                              message.Payload.DelayedPaymentBasepoint,
                                                              message.Payload.HtlcBasepoint,
                                                              message.Payload.FirstPerCommitmentPoint);

        BitcoinScript? localUpfrontShutdownScript = null;
        // Generate our upfront shutdown script
        if (_nodeOptions.Features.UpfrontShutdownScript > FeatureSupport.No)
        {
            // Generate our upfront shutdown script
            // TODO: Generate a script from the local key set
            // localUpfrontShutdownScript = ;
        }

        // Generate the channel configuration
        var useScidAlias = FeatureSupport.No;
        if (negotiatedFeatures.ScidAlias > FeatureSupport.No)
        {
            if (message.ChannelTypeTlv?.Features.IsFeatureSet(Feature.OptionScidAlias, true) ?? false)
                useScidAlias = FeatureSupport.Compulsory;
            else
                useScidAlias = FeatureSupport.Optional;
        }

        var channelConfig = new ChannelConfig(payload.ChannelReserveAmount, payload.FeeRatePerKw,
                                              payload.HtlcMinimumAmount, _nodeOptions.DustLimitAmount,
                                              payload.MaxAcceptedHtlcs, payload.MaxHtlcValueInFlight, minimumDepth,
                                              negotiatedFeatures.AnchorOutputs != FeatureSupport.No,
                                              payload.DustLimitAmount, payload.ToSelfDelay, useScidAlias,
                                              localUpfrontShutdownScript, remoteUpfrontShutdownScript);

        // Generate the commitment numbers
        var commitmentNumber = new CommitmentNumber(remoteKeySet.PaymentCompactBasepoint,
                                                    localKeySet.PaymentCompactBasepoint, _sha256);

        try
        {
            var fundingOutput = new FundingOutputInfo(payload.FundingAmount, localKeySet.FundingCompactPubKey,
                                                      remoteKeySet.FundingCompactPubKey);

            // Create the channel
            return new ChannelModel(channelConfig, payload.ChannelId, commitmentNumber, fundingOutput, false, null,
                                    null, toLocalAmount, localKeySet, 1, 0, toRemoteAmount, remoteKeySet, 1,
                                    remoteNodeId, 0, ChannelState.V1Opening, ChannelVersion.V1);
        }
        catch (Exception e)
        {
            throw new ChannelErrorException("Error creating commitment transaction", e);
        }
    }

    /// <summary>
    /// Conducts optional validation checks on channel parameters to ensure compliance with acceptable ranges
    /// and configurations beyond the mandatory requirements.
    /// </summary>
    /// <remarks>
    /// This method verifies that optional configuration parameters meet recommended safety and usability thresholds:
    /// - Validates that the funding amount meets the minimum channel size threshold.
    /// - Checks that the HTLC minimum amount is not excessively large relative to the node's configured minimum value.
    /// - Validates that the maximum HTLC value in flight is enough relative to the channel funds.
    /// - Ensures the channel reserve amount is not excessively high relative to the node's channel reserve configuration.
    /// - Verifies that the maximum number of accepted HTLCs meets a minimum threshold.
    /// - Confirms that the dust limit is not excessively large relative to the node's configured dust limit.
    /// </remarks>
    /// <param name="payload">The payload containing the channel's configuration parameters, including funding amount, HTLC limits, and related settings.</param>
    /// <exception cref="ChannelErrorException">
    /// Thrown when one of the optional checks fails, including missing channel type when required, insufficient funding,
    /// excessively high or low HTLC value limits, or incompatible reserve and dust limits.
    /// </exception>
    private void PerformOptionalChecks(OpenChannel1Payload payload)
    {
        // Check if Funding Satoshis is too small
        if (payload.FundingAmount < _nodeOptions.MinimumChannelSize)
            throw new ChannelErrorException($"Funding amount is too small: {payload.FundingAmount}");

        // Check if we consider htlc_minimum_msat too large. IE. 20% bigger than our htlc minimum amount
        if (payload.HtlcMinimumAmount > _nodeOptions.HtlcMinimumAmount * 1.2M)
            throw new ChannelErrorException($"Htlc minimum amount is too large: {payload.HtlcMinimumAmount}");

        // Check if we consider max_htlc_value_in_flight_msat too small. IE. 20% smaller than our maximum htlc value
        var maxHtlcValueInFlight =
            LightningMoney.Satoshis(_nodeOptions.AllowUpToPercentageOfChannelFundsInFlight *
                                    payload.FundingAmount.Satoshi / 100M);
        if (payload.MaxHtlcValueInFlight < maxHtlcValueInFlight * 0.8M)
            throw new ChannelErrorException($"Max htlc value in flight is too small: {payload.MaxHtlcValueInFlight}");

        // Check if we consider channel_reserve_satoshis too large. IE. 20% bigger than our channel reserve
        if (payload.ChannelReserveAmount > _nodeOptions.ChannelReserveAmount * 1.2M)
            throw new ChannelErrorException($"Channel reserve amount is too large: {payload.ChannelReserveAmount}");

        // Check if we consider max_accepted_htlcs too small. IE. 20% smaller than our max-accepted htlcs
        if (payload.MaxAcceptedHtlcs < (ushort)(_nodeOptions.MaxAcceptedHtlcs * 0.8M))
            throw new ChannelErrorException($"Max accepted htlcs is too small: {payload.MaxAcceptedHtlcs}");

        // Check if we consider dust_limit_satoshis too large. IE. 75% bigger than our dust limit
        if (payload.DustLimitAmount > _nodeOptions.DustLimitAmount * 1.75M)
            throw new ChannelErrorException($"Dust limit amount is too large: {payload.DustLimitAmount}");
    }

    /// <summary>
    /// Enforce mandatory checks when establishing a new Lightning Network channel.
    /// </summary>
    /// <remarks>
    /// The method validates channel parameters to ensure they comply with predefined safety and compatibility checks:
    /// - ChainHash must be compatible with the node's network.
    /// - Push amount must not exceed 1000 times the funding amount.
    /// - To_self_delay must not be unreasonably large compared to the node's configured value.
    /// - Max_accepted_htlcs must not exceed the allowed maximum.
    /// - Fee rate per kw must fall within acceptable limits.
    /// - Dust limit must be lower than or equal to the channel reserve amount and adhere to minimum thresholds.
    /// - Funding amount must be sufficient to cover fees and the channel reserve.
    /// - Large channels must only be supported if negotiated features include support for them.
    /// - Additional validation may apply to channel types based on negotiated options.
    /// </remarks>
    /// <param name="channelTypeTlv">Optional TLV data specifying the channel type, which may impose additional constraints.</param>
    /// <param name="currentFeeRatePerKw">The current network fee rate per kiloweight, used for fee validation.</param>
    /// <param name="negotiatedFeatures">Negotiated feature options between the participating nodes, affecting channel setup constraints.</param>
    /// <param name="payload">The payload containing the channel's configuration parameters and constraints.</param>
    /// <param name="minimumDepth">The minimum number of confirmations required for the channel to be considered operational.</param>
    /// <exception cref="ChannelErrorException">
    /// Thrown when any of the mandatory checks fail, such as invalid chain hash, excessive push amount, unreasonably large delay,
    /// invalid funding amount, unsupported large channel, or mismatched channel type.
    /// </exception>
    private void PerformMandatoryChecks(ChannelTypeTlv? channelTypeTlv, LightningMoney currentFeeRatePerKw,
                                        FeatureOptions negotiatedFeatures, OpenChannel1Payload payload,
                                        out uint minimumDepth)
    {
        // Check if ChainHash is compatible
        if (payload.ChainHash != _nodeOptions.BitcoinNetwork.ChainHash)
            throw new ChannelErrorException("ChainHash is not compatible");

        // Check if the push amount is too large
        if (payload.PushAmount > 1_000 * payload.FundingAmount)
            throw new ChannelErrorException($"Push amount is too large: {payload.PushAmount}");

        // Check if we consider to_self_delay unreasonably large. IE. 50% bigger than our to_self_delay
        if (payload.ToSelfDelay > _nodeOptions.ToSelfDelay * 1.5M)
            throw new ChannelErrorException($"To self delay is too large: {payload.ToSelfDelay}");

        // Check max_accepted_htlcs is too large
        if (payload.MaxAcceptedHtlcs > ChannelConstants.MaxAcceptedHtlcs)
            throw new ChannelErrorException($"Max accepted htlcs is too small: {payload.MaxAcceptedHtlcs}");

        // Check if we consider fee_rate_per_kw too large
        if (payload.FeeRatePerKw > ChannelConstants.MaxFeePerKw)
            throw new ChannelErrorException($"Fee rate per kw is too large: {payload.FeeRatePerKw}");

        // Check if we consider fee_rate_per_kw too small. IE. 20% smaller than our fee rate
        if (payload.FeeRatePerKw < ChannelConstants.MinFeePerKw || payload.FeeRatePerKw < currentFeeRatePerKw * 0.8M)
            throw new ChannelErrorException(
                $"Fee rate per kw is too small: {payload.FeeRatePerKw}, currentFee{currentFeeRatePerKw}");

        // Check if the dust limit is greater than the channel reserve amount 
        if (payload.DustLimitAmount > payload.ChannelReserveAmount)
            throw new ChannelErrorException(
                $"Dust limit({payload.DustLimitAmount}) is greater than channel reserve({payload.ChannelReserveAmount})");

        // Check if dust_limit_satoshis is too small
        if (payload.DustLimitAmount < ChannelConstants.MinDustLimitAmount)
            throw new ChannelErrorException($"Dust limit amount is too small: {payload.DustLimitAmount}");

        // Check if there are enough funds to pay for fees
        var expectedWeight = negotiatedFeatures.AnchorOutputs > FeatureSupport.No
                                 ? TransactionConstants.InitialCommitmentTransactionWeightNoAnchor
                                 : TransactionConstants.InitialCommitmentTransactionWeightWithAnchor;
        var expectedFee = LightningMoney.Satoshis(expectedWeight * currentFeeRatePerKw.Satoshi / 1000);
        if (payload.FundingAmount < expectedFee + payload.ChannelReserveAmount)
            throw new ChannelErrorException($"Funding amount is too small to cover fees: {payload.FundingAmount}");

        // Check if this is a large channel and if we support it
        if (payload.FundingAmount >= ChannelConstants.LargeChannelAmount &&
            negotiatedFeatures.LargeChannels == FeatureSupport.No)
            throw new ChannelErrorException("We don't support large channels");

        // Check ChannelType against negotiated options
        minimumDepth = _nodeOptions.MinimumDepth;
        if (channelTypeTlv is not null)
        {
            // Check if it set any non-negotiated features
            if (channelTypeTlv.Features.IsFeatureSet(Feature.OptionStaticRemoteKey, true))
            {
                if (negotiatedFeatures.StaticRemoteKey == FeatureSupport.No)
                    throw new ChannelErrorException("Static remote key feature is not supported but requested by peer");

                if (channelTypeTlv.Features.IsFeatureSet(Feature.OptionAnchorOutputs, true)
                 && negotiatedFeatures.AnchorOutputs == FeatureSupport.No)
                    throw new ChannelErrorException("Anchor outputs feature is not supported but requested by peer");

                if (channelTypeTlv.Features.IsFeatureSet(Feature.OptionScidAlias, true))
                {
                    if (payload.ChannelFlags.AnnounceChannel)
                        throw new ChannelErrorException("Invalid channel flags for OPTION_SCID_ALIAS");
                }

                // Check for ZeroConf feature
                if (channelTypeTlv.Features.IsFeatureSet(Feature.OptionZeroconf, true))
                {
                    if (_nodeOptions.Features.ZeroConf == FeatureSupport.No)
                        throw new ChannelErrorException("ZeroConf feature not supported but requested by peer");

                    minimumDepth = 0U;
                }
            }
        }
    }
}