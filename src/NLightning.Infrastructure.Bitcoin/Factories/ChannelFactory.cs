using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Factories;

using Domain.Protocol.Services;
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
using Protocol.Models;

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
            throw new ChannelErrorException("ChainHash is not compatible");

        // If dual fund is negotiated fail the channel
        if (negotiatedFeatures.DualFund == FeatureSupport.Compulsory)
            throw new ChannelErrorException("We can only accept dual fund channels");

        // Check if this is a large channel and if we support it
        if (payload.FundingAmount >= ChannelConstants.LARGE_CHANNEL_AMOUNT
            && negotiatedFeatures.LargeChannels == FeatureSupport.No
           ) throw new ChannelErrorException("We don't support large channels");

        // Check if the push amount is too large
        if (payload.PushAmount > 1_000 * payload.FundingAmount)
            throw new ChannelErrorException("Push amount is too large");

        // Check if dust_limit_satoshis is too small
        if (payload.DustLimitAmount < ChannelConstants.MIN_DUST_LIMIT_AMOUNT)
            throw new ChannelErrorException("Dust limit amount is too small");

        // Check if channel reserve amount is below the dust limit
        if (payload.DustLimitAmount > payload.ChannelReserveAmount)
            throw new ChannelErrorException("Channel reserve amount is too small");

        // Check if we consider channel_reserve_satoshis too large. IE. 20% bigger than our channel reserve
        if (payload.ChannelReserveAmount > _nodeOptions.ChannelReserveAmount * 1.2M)
            throw new ChannelErrorException("Channel reserve amount is too large");

        // Check channel flags for undefined bits
        if ((payload.ChannelFlags & ~1) != 0)
            _logger.LogWarning("[{methodName}] ChannelId: {channelId} | Channel flags have undefined bits set",
                               nameof(CreateChannelV1AsNonInitiator), payload.ChannelId);

        // Check for the upfront shutdown script
        if (negotiatedFeatures.UpfrontShutdownScript == FeatureSupport.Compulsory
            && message.UpfrontShutdownScriptTlv?.Length == 0
           ) throw new ChannelErrorException("Upfront shutdown script is required but not provided");

        // Check if the option channel type was negotiated and channel type is present
        if (negotiatedFeatures.ChannelType == FeatureSupport.Compulsory && message.ChannelTypeTlv is null)
            throw new ChannelErrorException("Channel type was negotiated but not provided");

        // Check ChannelType against negotiated options
        var minimumDepth = _nodeOptions.MinimumDepth;
        if (message.ChannelTypeTlv is not null)
        {
            // Check if it set any non-negotiated features
            if (!negotiatedFeatures.GetNodeFeatures().IsCompatible(message.ChannelTypeTlv.Features,
                                                                  out var negotiatedChannelFeatures)
                || negotiatedChannelFeatures is null
               ) throw new ChannelErrorException("Negotiated channel features are not compatible with channel type");

            // Check if channel should be announced but option scid alias is negotiated
            if (negotiatedChannelFeatures.IsFeatureSet(Feature.OptionScidAlias)
                && payload.ChannelFlags.AnnounceChannel
               ) throw new ChannelErrorException("Invalid channel flags for OPTION_SCID_ALIAS");

            // Check for ZeroConf feature
            if (message.ChannelTypeTlv.Features.IsFeatureSet(Feature.OptionZeroconf, true))
            {
                if (_nodeOptions.Features.ZeroConf == FeatureSupport.No)
                {
                    throw new ChannelErrorException("ZeroConf feature not supported but requested by peer");
                }

                minimumDepth = 0U;
            }
        }

        // Check if Funding Satoshis is too small
        if (_nodeOptions.MinimumChannelSize > payload.FundingAmount)
            throw new ChannelErrorException("Funding amount is too small");

        // Check if we consider htlc_minimum_msat too large. IE. 20% bigger than our htlc minimum amount
        if (payload.HtlcMinimumAmount > _nodeOptions.HtlcMinimumAmount * 1.2M)
            throw new ChannelErrorException("Htlc minimum amount is too large");

        // Check if we consider max_htlc_value_in_flight_msat too small. IE. 20% smaller than our max htlc value
        if (payload.MaxHtlcValueInFlight < _nodeOptions.MaxHtlcValueInFlight * 0.8M)
            throw new ChannelErrorException("Max htlc value in flight is too small");

        // Check if we consider max_accepted_htlcs too small. IE. 20% smaller than our max accepted htlcs
        if (payload.MaxAcceptedHtlcs < (ushort)(_nodeOptions.MaxAcceptedHtlcs * 0.8M))
            throw new ChannelErrorException("Max accepted htlcs is too small");

        // Check max_accepted_htlcs is too large
        if (payload.MaxAcceptedHtlcs > ChannelConstants.MAX_ACCEPTED_HTLCS)
            throw new ChannelErrorException("Max accepted htlcs is too small");

        // Check if we consider dust_limit_satoshis too large. IE. 20% bigger than our dust limit
        if (payload.DustLimitAmount > _nodeOptions.DustLimitAmount * 1.2M)
            throw new ChannelErrorException("Dust limit amount is too large");

        // Check if there's enough funds to pay for fees
        if (payload.FundingAmount < payload.DustLimitAmount + payload.ChannelReserveAmount)
            throw new ChannelErrorException("Funding amount is too small");

        // Check if we consider to_self_delay unreasonably large. IE. 50% bigger than our to_self_delay
        if (payload.ToSelfDelay > _nodeOptions.ToSelfDelay * 1.5M)
            throw new ChannelErrorException("To self delay is too large");

        // Check if we consider fee_rate_per_kw too large
        if (payload.FeeRatePerKw > ChannelConstants.MAX_FEE_PER_KW)
            throw new ChannelErrorException("Fee rate per kw is too large");

        // Check if we consider fee_rate_per_kw too small. IE. 20% smaller than our fee rate
        if (payload.FeeRatePerKw < ChannelConstants.MIN_FEE_PER_KW
            || payload.FeeRatePerKw < _feeService.GetCachedFeeRatePerKw() * 0.8M
           ) throw new ChannelErrorException("Fee rate per kw is too small");
        #endregion

        // Set peer options as agreed
        var peerOptions = new NodeOptions
        {
            Network = _nodeOptions.Network,
            DustLimitAmount = payload.DustLimitAmount,
            ChannelReserveAmount = payload.ChannelReserveAmount,
            ToSelfDelay = payload.ToSelfDelay,
            MaxHtlcValueInFlight = payload.MaxHtlcValueInFlight,
            HtlcMinimumAmount = payload.HtlcMinimumAmount,
            MaxAcceptedHtlcs = payload.MaxAcceptedHtlcs,
            Features = negotiatedFeatures
        };

        // Calculate the amounts
        var toLocalAmount = payload.PushAmount;
        var toRemoteAmount = payload.FundingAmount - payload.PushAmount;

        // Generate a private key for this channel
        var channelKey = _secureKeyManager.GetNextKey(out var index);

        // Generate secrets
        var firstPerCommitmentSecretBytes = _keyDerivationService
            .GeneratePerCommitmentSecret(channelKey.PrivateKey.ToBytes(), CryptoConstants.FIRST_PER_COMMITMENT_INDEX);
        using var firstPerCommitmentSecret = new Key(firstPerCommitmentSecretBytes);
        using var localFundingSecret = channelKey.PrivateKey;
        // using var localRevocationSecret = channelKey.Derive(0).PrivateKey;
        using var localPaymentSecret = channelKey.Derive(1).PrivateKey;
        // using var localDelayedPaymentSecret = channelKey.Derive(2).PrivateKey;
        // using var localHtlcSecret = channelKey.Derive(3).PrivateKey;
        var bitcoinSecret = new BitcoinSecret(localPaymentSecret, _nodeOptions.Network);

        // Generate Basepoints
        // var firstPerCommitmentBasepoint = firstPerCommitmentSecret.PubKey;
        var localFundingPubKey = localFundingSecret.PubKey;
        // var localRevocationBasepoint = localRevocationSecret.PubKey;
        var localPaymentBasepoint = localPaymentSecret.PubKey;
        // var localDelayedPaymentBasepoint = localDelayedPaymentSecret.PubKey;
        // var localHtlcBasepoint = localHtlcSecret.PubKey;

        // Create a per-commitment storage
        var ourPerCommitmentStorage = new SecretStorageService();
        ourPerCommitmentStorage.InsertSecret(firstPerCommitmentSecretBytes, CryptoConstants.FIRST_PER_COMMITMENT_INDEX);

        // Generate the commitment number
        var commitmentNumber = new CommitmentNumber(localPaymentBasepoint, payload.PaymentBasepoint);

        // Create a commitment transaction
        try
        {
            var fundingOutput = new FundingOutput(localFundingPubKey, payload.FundingPubKey, payload.FundingAmount);
            var commitmentTx = _commitmentTransactionFactory
                .CreateCommitmentTransactionAsResponder(peerOptions, fundingOutput, payload.PaymentBasepoint,
                    localPaymentBasepoint, payload.DelayedPaymentBasepoint,
                    payload.RevocationBasepoint, toRemoteAmount, toLocalAmount,
                    commitmentNumber, bitcoinSecret);
        var fundingOutput = new FundingOutput(localFundingPubKey, payload.FundingPubKey, payload.FundingAmount);
        var commitmentTx = _commitmentTransactionFactory
            .CreateCommitmentTransaction(peerOptions, fundingOutput, localPaymentBasepoint, payload.PaymentBasepoint, 
                                         localDelayedPaymentBasepoint, payload.RevocationBasepoint, toLocalAmount, 
                                         toRemoteAmount, commitmentNumber, false, bitcoinSecret);

            // TODO: Check if amount is enough to pay for fees

            // TODO: Check if to_local and to_remote amounts are greater than channel reserve
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating commitment transaction");
        }

        return new Channel(payload.ChannelId, payload.FirstPerCommitmentPoint, false, index, minimumDepth,
                           payload.FundingPubKey, message.UpfrontShutdownScriptTlv?.ShutdownScriptPubkey,
                           ourPerCommitmentStorage);
    }
}