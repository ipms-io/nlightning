using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Factories;

using Domain.Protocol.Constants;
using Domain.Protocol.Factories;
using Domain.Protocol.Messages;
using Domain.Protocol.Messages.Interfaces;
using Interfaces;
using Messages.Types;

public class MessageTypeSerializerFactory : IMessageTypeSerializerFactory
{
    private readonly Dictionary<Type, IMessageTypeSerializer> _serializers = new();
    private readonly Dictionary<MessageTypes, Type> _messageTypeDictionary = new();
    private readonly IPayloadSerializerFactory _payloadSerializerFactory;
    private readonly ITlvConverterFactory _tlvConverterFactory;
    private readonly ITlvStreamSerializer _tlvStreamSerializer;

    public MessageTypeSerializerFactory(IPayloadSerializerFactory payloadSerializerFactory,
                                        ITlvConverterFactory tlvConverterFactory,
                                        ITlvStreamSerializer tlvStreamSerializer)
    {
        _payloadSerializerFactory = payloadSerializerFactory;
        _tlvConverterFactory = tlvConverterFactory;
        _tlvStreamSerializer = tlvStreamSerializer;

        RegisterSerializers();
        RegisterTypeDictionary();
    }

    public IMessageTypeSerializer<TMessageType>? GetSerializer<TMessageType>() where TMessageType : IMessage
    {
        return _serializers.GetValueOrDefault(typeof(TMessageType)) as IMessageTypeSerializer<TMessageType>;
    }

    public IMessageTypeSerializer? GetSerializer(MessageTypes messageType)
    {
        var type = _messageTypeDictionary.GetValueOrDefault(messageType);
        if (type is null)
            return null;

        return _serializers.GetValueOrDefault(type);
    }

    private void RegisterSerializers()
    {
        _serializers.Add(typeof(AcceptChannel1Message),
                         new AcceptChannel1MessageTypeSerializer(_payloadSerializerFactory, _tlvConverterFactory,
                                                                 _tlvStreamSerializer));
        _serializers.Add(typeof(AcceptChannel2Message),
                         new AcceptChannel2MessageTypeSerializer(_payloadSerializerFactory, _tlvConverterFactory,
                                                                 _tlvStreamSerializer));
        _serializers.Add(typeof(ChannelReadyMessage),
                         new ChannelReadyMessageTypeSerializer(_payloadSerializerFactory, _tlvConverterFactory,
                                                               _tlvStreamSerializer));
        _serializers.Add(typeof(ChannelReestablishMessage),
                         new ChannelReestablishMessageTypeSerializer(_payloadSerializerFactory,
                                                                     _tlvConverterFactory, _tlvStreamSerializer));
        _serializers.Add(typeof(ClosingSignedMessage),
                         new ClosingSignedMessageTypeSerializer(_payloadSerializerFactory, _tlvConverterFactory,
                                                                _tlvStreamSerializer));
        _serializers.Add(typeof(CommitmentSignedMessage),
                         new CommitmentSignedMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(ErrorMessage), new ErrorMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(FundingCreatedMessage),
                         new FundingCreatedMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(FundingSignedMessage),
                         new FundingSignedMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(InitMessage),
                         new InitMessageTypeSerializer(_payloadSerializerFactory, _tlvConverterFactory,
                                                       _tlvStreamSerializer));
        _serializers.Add(typeof(OpenChannel1Message),
                         new OpenChannel1MessageTypeSerializer(_payloadSerializerFactory, _tlvConverterFactory,
                                                               _tlvStreamSerializer));
        _serializers.Add(typeof(OpenChannel2Message),
                         new OpenChannel2MessageTypeSerializer(_payloadSerializerFactory, _tlvConverterFactory,
                                                               _tlvStreamSerializer));
        _serializers.Add(typeof(PingMessage), new PingMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(PongMessage), new PongMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(RevokeAndAckMessage),
                         new RevokeAndAckMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(ShutdownMessage), new ShutdownMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(StfuMessage), new StfuMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(TxAbortMessage), new TxAbortMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(TxAckRbfMessage),
                         new TxAckRbfMessageTypeSerializer(_payloadSerializerFactory, _tlvConverterFactory,
                                                           _tlvStreamSerializer));
        _serializers.Add(typeof(TxAddInputMessage), new TxAddInputMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(TxAddOutputMessage),
                         new TxAddOutputMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(TxCompleteMessage), new TxCompleteMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(TxInitRbfMessage),
                         new TxInitRbfMessageTypeSerializer(_payloadSerializerFactory, _tlvConverterFactory,
                                                            _tlvStreamSerializer));
        _serializers.Add(typeof(TxRemoveInputMessage),
                         new TxRemoveInputMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(TxRemoveOutputMessage),
                         new TxRemoveOutputMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(TxSignaturesMessage),
                         new TxSignaturesMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(UpdateAddHtlcMessage),
                         new UpdateAddHtlcMessageTypeSerializer(_payloadSerializerFactory,
                                                                _tlvConverterFactory, _tlvStreamSerializer));
        _serializers.Add(typeof(UpdateFailHtlcMessage),
                         new UpdateFailHtlcMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(UpdateFailMalformedHtlcMessage),
                         new UpdateFailMalformedHtlcMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(UpdateFeeMessage), new UpdateFeeMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(UpdateFulfillHtlcMessage),
                         new UpdateFulfillHtlcMessageTypeSerializer(_payloadSerializerFactory));
        _serializers.Add(typeof(WarningMessage), new WarningMessageTypeSerializer(_payloadSerializerFactory));
    }

    private void RegisterTypeDictionary()
    {
        _messageTypeDictionary.Add(MessageTypes.AcceptChannel, typeof(AcceptChannel1Message));
        _messageTypeDictionary.Add(MessageTypes.AcceptChannel2, typeof(AcceptChannel2Message));
        _messageTypeDictionary.Add(MessageTypes.ChannelReady, typeof(ChannelReadyMessage));
        _messageTypeDictionary.Add(MessageTypes.ChannelReestablish, typeof(ChannelReestablishMessage));
        _messageTypeDictionary.Add(MessageTypes.ClosingSigned, typeof(ClosingSignedMessage));
        _messageTypeDictionary.Add(MessageTypes.CommitmentSigned, typeof(CommitmentSignedMessage));
        _messageTypeDictionary.Add(MessageTypes.Error, typeof(ErrorMessage));
        _messageTypeDictionary.Add(MessageTypes.FundingCreated, typeof(FundingCreatedMessage));
        _messageTypeDictionary.Add(MessageTypes.FundingSigned, typeof(FundingSignedMessage));
        _messageTypeDictionary.Add(MessageTypes.Init, typeof(InitMessage));
        _messageTypeDictionary.Add(MessageTypes.OpenChannel, typeof(OpenChannel1Message));
        _messageTypeDictionary.Add(MessageTypes.OpenChannel2, typeof(OpenChannel2Message));
        _messageTypeDictionary.Add(MessageTypes.Ping, typeof(PingMessage));
        _messageTypeDictionary.Add(MessageTypes.Pong, typeof(PingMessage));
        _messageTypeDictionary.Add(MessageTypes.RevokeAndAck, typeof(RevokeAndAckMessage));
        _messageTypeDictionary.Add(MessageTypes.Shutdown, typeof(ShutdownMessage));
        _messageTypeDictionary.Add(MessageTypes.Stfu, typeof(StfuMessage));
        _messageTypeDictionary.Add(MessageTypes.TxAbort, typeof(TxAbortMessage));
        _messageTypeDictionary.Add(MessageTypes.TxAckRbf, typeof(TxAckRbfMessage));
        _messageTypeDictionary.Add(MessageTypes.TxAddInput, typeof(TxAddInputMessage));
        _messageTypeDictionary.Add(MessageTypes.TxAddOutput, typeof(TxAddOutputMessage));
        _messageTypeDictionary.Add(MessageTypes.TxComplete, typeof(TxCompleteMessage));
        _messageTypeDictionary.Add(MessageTypes.TxInitRbf, typeof(TxInitRbfMessage));
        _messageTypeDictionary.Add(MessageTypes.TxRemoveInput, typeof(TxRemoveInputMessage));
        _messageTypeDictionary.Add(MessageTypes.TxRemoveOutput, typeof(TxRemoveOutputMessage));
        _messageTypeDictionary.Add(MessageTypes.TxSignatures, typeof(TxSignaturesMessage));
        _messageTypeDictionary.Add(MessageTypes.UpdateAddHtlc, typeof(UpdateAddHtlcMessage));
        _messageTypeDictionary.Add(MessageTypes.UpdateFailHtlc, typeof(UpdateFailHtlcMessage));
        _messageTypeDictionary.Add(MessageTypes.UpdateFailMalformedHtlc, typeof(UpdateFailMalformedHtlcMessage));
        _messageTypeDictionary.Add(MessageTypes.UpdateFee, typeof(UpdateFeeMessage));
        _messageTypeDictionary.Add(MessageTypes.UpdateFulfillHtlc, typeof(UpdateFulfillHtlcMessage));
        _messageTypeDictionary.Add(MessageTypes.Warning, typeof(WarningMessage));
    }
}