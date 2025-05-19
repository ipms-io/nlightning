namespace NLightning.Infrastructure.Serialization.Factories;

using Domain.Protocol.Constants;
using Domain.Protocol.Factories;
using Domain.Protocol.Messages;
using Domain.Protocol.Messages.Interfaces;
using Domain.Serialization.Factories;
using Domain.Serialization.Messages.Types;
using Interfaces;
using Messages.Types;

public class MessageTypeSerializerFactory : IMessageTypeSerializerFactory
{
    private readonly Dictionary<Type, IMessageTypeSerializer> _serializers = new();
    private readonly Dictionary<ushort, Type> _ushortTypeDictionary = new();
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

    public IMessageTypeSerializer? GetSerializer(ushort messageType)
    {
        var type = _ushortTypeDictionary.GetValueOrDefault(messageType);
        if (type is null)
            return null;

        return _serializers.GetValueOrDefault(type);
    }

    private void RegisterSerializers()
    {
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
        _serializers.Add(typeof(InitMessage),
                         new InitMessageTypeSerializer(_payloadSerializerFactory, _tlvConverterFactory,
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
        _ushortTypeDictionary.Add(MessageTypes.ACCEPT_CHANNEL_2, typeof(AcceptChannel2Message));
        _ushortTypeDictionary.Add(MessageTypes.CHANNEL_READY, typeof(ChannelReadyMessage));
        _ushortTypeDictionary.Add(MessageTypes.CHANNEL_REESTABLISH, typeof(ChannelReestablishMessage));
        _ushortTypeDictionary.Add(MessageTypes.CLOSING_SIGNED, typeof(ClosingSignedMessage));
        _ushortTypeDictionary.Add(MessageTypes.COMMITMENT_SIGNED, typeof(CommitmentSignedMessage));
        _ushortTypeDictionary.Add(MessageTypes.ERROR, typeof(ErrorMessage));
        _ushortTypeDictionary.Add(MessageTypes.INIT, typeof(InitMessage));
        _ushortTypeDictionary.Add(MessageTypes.OPEN_CHANNEL_2, typeof(OpenChannel2Message));
        _ushortTypeDictionary.Add(MessageTypes.PING, typeof(PingMessage));
        _ushortTypeDictionary.Add(MessageTypes.PONG, typeof(PingMessage));
        _ushortTypeDictionary.Add(MessageTypes.REVOKE_AND_ACK, typeof(RevokeAndAckMessage));
        _ushortTypeDictionary.Add(MessageTypes.SHUTDOWN, typeof(ShutdownMessage));
        _ushortTypeDictionary.Add(MessageTypes.STFU, typeof(StfuMessage));
        _ushortTypeDictionary.Add(MessageTypes.TX_ABORT, typeof(TxAbortMessage));
        _ushortTypeDictionary.Add(MessageTypes.TX_ACK_RBF, typeof(TxAckRbfMessage));
        _ushortTypeDictionary.Add(MessageTypes.TX_ADD_INPUT, typeof(TxAddInputMessage));
        _ushortTypeDictionary.Add(MessageTypes.TX_ADD_OUTPUT, typeof(TxAddOutputMessage));
        _ushortTypeDictionary.Add(MessageTypes.TX_COMPLETE, typeof(TxCompleteMessage));
        _ushortTypeDictionary.Add(MessageTypes.TX_INIT_RBF, typeof(TxInitRbfMessage));
        _ushortTypeDictionary.Add(MessageTypes.TX_REMOVE_INPUT, typeof(TxRemoveInputMessage));
        _ushortTypeDictionary.Add(MessageTypes.TX_REMOVE_OUTPUT, typeof(TxRemoveOutputMessage));
        _ushortTypeDictionary.Add(MessageTypes.TX_SIGNATURES, typeof(TxSignaturesMessage));
        _ushortTypeDictionary.Add(MessageTypes.UPDATE_ADD_HTLC, typeof(UpdateAddHtlcMessage));
        _ushortTypeDictionary.Add(MessageTypes.UPDATE_FAIL_HTLC, typeof(UpdateFailHtlcMessage));
        _ushortTypeDictionary.Add(MessageTypes.UPDATE_FAIL_MALFORMED_HTLC, typeof(UpdateFailMalformedHtlcMessage));
        _ushortTypeDictionary.Add(MessageTypes.UPDATE_FEE, typeof(UpdateFeeMessage));
        _ushortTypeDictionary.Add(MessageTypes.UPDATE_FULFILL_HTLC, typeof(UpdateFulfillHtlcMessage));
        _ushortTypeDictionary.Add(MessageTypes.WARNING, typeof(WarningMessage));
    }
}