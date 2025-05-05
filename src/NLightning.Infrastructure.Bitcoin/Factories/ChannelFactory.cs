using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Factories;

using Domain.Bitcoin.Factories;
using Domain.Bitcoin.Services;
using Domain.Channels;
using Domain.Crypto.Constants;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Node.Options;
using Domain.Protocol.Constants;
using Domain.Protocol.Factories;
using Domain.Protocol.Managers;
using Domain.Protocol.Messages;
using Infrastructure.Protocol.Services;
using Interfaces;
using Outputs;

public class ChannelFactory : IChannelFactory
{
    private readonly ICommitmentTransactionFactory _commitmentTransactionFactory;
    private readonly IFeeService _feeService;
    private readonly IFundingTransactionFactory _fundingTransactionFactory;
    private readonly IKeyDerivationService _keyDerivationService;
    private readonly ILogger<ChannelFactory> _logger;
    private readonly NodeOptions _nodeOptions;
    private readonly ISecureKeyManager _secureKeyManager;

    public ChannelFactory(ICommitmentTransactionFactory commitmentTransactionFactory, IFeeService feeService,
                          IFundingTransactionFactory fundingTransactionFactory,
                          IKeyDerivationService keyDerivationService, ILogger<ChannelFactory> logger,
                          IOptions<NodeOptions> nodeOptions, ISecureKeyManager secureKeyManager)
    {
        _commitmentTransactionFactory = commitmentTransactionFactory;
        _feeService = feeService;
        _fundingTransactionFactory = fundingTransactionFactory;
        _keyDerivationService = keyDerivationService;
        _logger = logger;
        _nodeOptions = nodeOptions.Value;
        _secureKeyManager = secureKeyManager;
    }

    public Channel CreateChannelV1AsNonInitiator(OpenChannel1Message message, FeatureOptions negotiatedFeatures)
    {
        var payload = message.Payload;

        #region Checks
        // Check if ChainHash is compatible
        if (payload.ChainHash != _nodeOptions.Network.ChainHash)
            throw new ChannelException("ChainHash is not compatible");

        // If dual fund is negotiated fail the channel
        if (negotiatedFeatures.DualFund == FeatureSupport.Compulsory)
            throw new ChannelException("We can only accept dual fund channels");

        // Check if this is a large channel and if we support it
        if (payload.FundingAmount >= ChannelConstants.LARGE_CHANNEL_AMOUNT
            && negotiatedFeatures.LargeChannels == FeatureSupport.No
           ) throw new ChannelException("We don't support large channels");

        // Check if push amount is too large
        if (payload.PushAmount > 1_000 * payload.FundingAmount)
            throw new ChannelException("Push amount is too large");

        // Check if dust_limit_satoshis is too small
        if (payload.DustLimitAmount < ChannelConstants.MIN_DUST_LIMIT_AMOUNT)
            throw new ChannelException("Dust limit amount is too small");

        // Check if channel reserve amount is below the dust limit
        if (payload.DustLimitAmount > payload.ChannelReserveAmount)
            throw new ChannelException("Channel reserve amount is too small");

        // Check if we consider channel_reserve_satoshis too large. IE. 20% bigger than our channel reserve
        if (payload.ChannelReserveAmount > _nodeOptions.ChannelReserveAmount * 1.2M)
            throw new ChannelException("Channel reserve amount is too large");

        // Check channel flags for undefined bits
        if ((payload.ChannelFlags & ~1) != 0)
            _logger.LogWarning("[{methodName}] ChannelId: {channelId} | Channel flags have undefined bits set",
                               nameof(CreateChannelV1AsNonInitiator), payload.ChannelId);

        // Check for upfront shutdown script
        if (negotiatedFeatures.UpfrontShutdownScript == FeatureSupport.Compulsory
            && message.UpfrontShutdownScriptTlv?.Length == 0
           ) throw new ChannelException("Upfront shutdown script is required but not provided");

        // Check if option channel type was negotiated and channel type is present
        if (negotiatedFeatures.ChannelType == FeatureSupport.Compulsory && message.ChannelTypeTlv is null)
            throw new ChannelException("Channel type was negotiated but not provided");

        // Check ChannelType against negotiated options
        var minimumDepth = _nodeOptions.MinimumDepth;
        if (message.ChannelTypeTlv is not null)
        {
            // Check if it set any non-negotiated features
            if (!negotiatedFeatures.GetNodeFeatures().IsCompatible(message.ChannelTypeTlv.Features,
                                                                  out var negotiatedChannelFeatures)
                || negotiatedChannelFeatures is null
               ) throw new ChannelException("Negotiated channel features are not compatible with channel type");

            // Check if channel should be announced but option scid alias is negotiated
            if (negotiatedChannelFeatures.IsFeatureSet(Feature.OptionScidAlias)
                && payload.ChannelFlags.AnnounceChannel
               ) throw new ChannelException("Invalid channel flags for OPTION_SCID_ALIAS");

            // Check for ZeroConf feature
            if (message.ChannelTypeTlv.Features.IsFeatureSet(Feature.OptionZeroconf, true))
            {
                if (_nodeOptions.Features.ZeroConf == FeatureSupport.No)
                {
                    throw new ChannelException("ZeroConf feature not supported but requested by peer");
                }

                minimumDepth = 0U;
            }
        }

        // Check if Funding Satoshis is too small
        if (_nodeOptions.MinimumChannelSize > payload.FundingAmount)
            throw new ChannelException("Funding amount is too small");

        // Check if we consider htlc_minimum_msat too large. IE. 20% bigger than our htlc minimum amount
        if (payload.HtlcMinimumAmount > _nodeOptions.HtlcMinimumAmount * 1.2M)
            throw new ChannelException("Htlc minimum amount is too large");

        // Check if we consider max_htlc_value_in_flight_msat too small. IE. 20% smaller than our max htlc value
        if (payload.MaxHtlcValueInFlight < _nodeOptions.MaxHtlcValueInFlight * 0.8M)
            throw new ChannelException("Max htlc value in flight is too small");

        // Check if we consider max_accepted_htlcs too small. IE. 20% smaller than our max accepted htlcs
        if (payload.MaxAcceptedHtlcs < (ushort)(_nodeOptions.MaxAcceptedHtlcs * 0.8M))
            throw new ChannelException("Max accepted htlcs is too small");

        // Check max_accepted_htlcs is too large
        if (payload.MaxAcceptedHtlcs > ChannelConstants.MAX_ACCEPTED_HTLCS)
            throw new ChannelException("Max accepted htlcs is too small");

        // Check if we consider dust_limit_satoshis too large. IE. 20% bigger than our dust limit
        if (payload.DustLimitAmount > _nodeOptions.DustLimitAmount * 1.2M)
            throw new ChannelException("Dust limit amount is too large");

        // Check if there's enough funds to pay for fees
        if (payload.FundingAmount < payload.DustLimitAmount + payload.ChannelReserveAmount)
            throw new ChannelException("Funding amount is too small");

        // Check if we consider to_self_delay unreasonably large. IE. 50% bigger than our to_self_delay
        if (payload.ToSelfDelay > _nodeOptions.ToSelfDelay * 1.5M)
            throw new ChannelException("To self delay is too large");

        // Check if we consider fee_rate_per_kw too large
        if (payload.FeeRatePerKw > ChannelConstants.MAX_FEE_PER_KW)
            throw new ChannelException("Fee rate per kw is too large");

        // Check if we consider fee_rate_per_kw too small. IE. 20% smaller than our fee rate
        if (payload.FeeRatePerKw < ChannelConstants.MIN_FEE_PER_KW
            || payload.FeeRatePerKw < _feeService.GetCachedFeeRatePerKw() * 0.8M
           ) throw new ChannelException("Fee rate per kw is too small");
        #endregion

        // Generate a private key for this channel
        var channelKey = _secureKeyManager.GetNextKey(out var index);
        
        var firstPerCommitmentSecret = _keyDerivationService
            .GeneratePerCommitmentSecret(channelKey.ToBytes(), CryptoConstants.FIRST_PER_COMMITMENT_INDEX);
        var firstPerCommitmentBasePoint = new Key(firstPerCommitmentSecret).PubKey;
        
        var localFundingPubKey = channelKey.Neuter().PubKey;
        var revocationBasePoint = channelKey.Derive(0).Neuter().PubKey;
        var paymentBasePoint = channelKey.Derive(1).Neuter().PubKey;
        var delayedPaymentBasePoint = channelKey.Derive(2).Neuter().PubKey;
        var htlcBasePoint = channelKey.Derive(3).Neuter().PubKey;
        
        // Create a per-commitment storage
        var ourPerCommitmentStorage = new SecretStorageService();
        ourPerCommitmentStorage.InsertSecret(firstPerCommitmentSecret, CryptoConstants.FIRST_PER_COMMITMENT_INDEX);

        // Create a commitment transaction
        var fundingOutput = new FundingOutput(localFundingPubKey, payload.FundingPubKey, payload.FundingAmount);
        var commitmentTx = _commitmentTransactionFactory
            .CreateCommitmentTransaction(peerOptions, fundingOutput, localPaymentBasepoint, payload.PaymentBasepoint, 
                                         localDelayedPaymentBasepoint, payload.RevocationBasepoint, toLocalAmount, 
                                         toRemoteAmount, commitmentNumber, false, bitcoinSecret);

        // TODO: Check if amount is enough to pay for fees

        // TODO: Check if to_local and to_remote amounts are greater than channel reserve

        return new Channel(payload.ChannelId, payload.FirstPerCommitmentPoint, false, index, minimumDepth,
                                  payload.FundingPubKey, message.UpfrontShutdownScriptTlv?.ShutdownScriptPubkey,
                                  ourPerCommitmentStorage);
    }
}