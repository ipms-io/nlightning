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
    private readonly IPayloadTypeSerializerFactory _payloadTypeSerializerFactory;
    private readonly ITlvConverterFactory _tlvConverterFactory;
    private readonly ITlvStreamSerializer _tlvStreamSerializer;

    public MessageTypeSerializerFactory(IPayloadTypeSerializerFactory payloadTypeSerializerFactory,
                                        ITlvConverterFactory tlvConverterFactory,
                                        ITlvStreamSerializer tlvStreamSerializer)
    {
        _payloadTypeSerializerFactory = payloadTypeSerializerFactory;
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
                         new AcceptChannel2MessageTypeSerializer(_payloadTypeSerializerFactory, _tlvConverterFactory,
                                                                 _tlvStreamSerializer));
        _serializers.Add(typeof(ChannelReadyMessage),
                         new ChannelReadyMessageTypeSerializer(_payloadTypeSerializerFactory, _tlvConverterFactory,
                                                               _tlvStreamSerializer));
        _serializers.Add(typeof(ChannelReestablishMessage),
                         new ChannelReestablishMessageTypeSerializer(_payloadTypeSerializerFactory,
                                                                     _tlvConverterFactory, _tlvStreamSerializer));
        _serializers.Add(typeof(ClosingSignedMessage),
                         new ClosingSignedMessageTypeSerializer(_payloadTypeSerializerFactory, _tlvConverterFactory, 
                                                                _tlvStreamSerializer));
        _serializers.Add(typeof(CommitmentSignedMessage),
                         new CommitmentSignedMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(ErrorMessage), new ErrorMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(InitMessage),
                         new InitMessageTypeSerializer(_payloadTypeSerializerFactory, _tlvConverterFactory,
                                                       _tlvStreamSerializer));
        _serializers.Add(typeof(OpenChannel2Message),
                         new OpenChannel2MessageTypeSerializer(_payloadTypeSerializerFactory, _tlvConverterFactory,
                                                               _tlvStreamSerializer));
        _serializers.Add(typeof(PingMessage), new PingMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(PongMessage), new PongMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(RevokeAndAckMessage), 
                         new RevokeAndAckMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(ShutdownMessage), new ShutdownMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(StfuMessage), new StfuMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(TxAbortMessage), new TxAbortMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(TxAckRbfMessage),
                         new TxAckRbfMessageTypeSerializer(_payloadTypeSerializerFactory, _tlvConverterFactory,
                                                           _tlvStreamSerializer));
        _serializers.Add(typeof(TxAddInputMessage), new TxAddInputMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(TxAddOutputMessage),
                         new TxAddOutputMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(TxCompleteMessage), new TxCompleteMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(TxInitRbfMessage),
                         new TxInitRbfMessageTypeSerializer(_payloadTypeSerializerFactory, _tlvConverterFactory,
                                                            _tlvStreamSerializer));
        _serializers.Add(typeof(TxRemoveInputMessage),
                         new TxRemoveInputMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(TxRemoveOutputMessage),
                         new TxRemoveOutputMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(TxSignaturesMessage),
                         new TxSignaturesMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(UpdateAddHtlcMessage), 
                         new UpdateAddHtlcMessageTypeMessageTypeSerializer(_payloadTypeSerializerFactory,
                                                                           _tlvConverterFactory, _tlvStreamSerializer));
        _serializers.Add(typeof(UpdateFailHtlcMessage),
                         new UpdateFailHtlcMessageTypeMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(UpdateFailMalformedHtlcMessage),
                         new UpdateFailMalformedHtlcMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(UpdateFeeMessage), new UpdateFeeMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(UpdateFulfillHtlcMessage),
                         new UpdateFulfillHtlcMessageTypeSerializer(_payloadTypeSerializerFactory));
        _serializers.Add(typeof(WarningMessage), new WarningMessageTypeSerializer(_payloadTypeSerializerFactory));
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