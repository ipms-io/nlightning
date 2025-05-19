namespace NLightning.Infrastructure.Serialization.Factories;

using Domain.Protocol.Constants;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.Serialization.Factories;
using Domain.Serialization.Payloads;
using Interfaces;
using Payloads;

public class PayloadSerializerFactory : IPayloadSerializerFactory
{
    private readonly IFeatureSetSerializer _featureSetSerializer;
    private readonly Dictionary<Type, IPayloadSerializer> _serializers = new();
    private readonly Dictionary<ushort, Type> _ushortTypeDictionary = new();
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public PayloadSerializerFactory(IFeatureSetSerializer featureSetSerializer,
                                    IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _featureSetSerializer = featureSetSerializer;
        _valueObjectSerializerFactory = valueObjectSerializerFactory;

        RegisterSerializers();
        RegisterTypeDictionary();
    }

    public IPayloadSerializer<TPayloadType>? GetSerializer<TPayloadType>() where TPayloadType : IMessagePayload
    {
        return _serializers.GetValueOrDefault(typeof(TPayloadType)) as IPayloadSerializer<TPayloadType>;
    }

    public IPayloadSerializer? GetSerializer(ushort messageType)
    {
        var type = _ushortTypeDictionary.GetValueOrDefault(messageType);
        return type is null ? null : _serializers.GetValueOrDefault(type);
    }

    private void RegisterSerializers()
    {
        _serializers.Add(typeof(AcceptChannel2Payload),
                         new AcceptChannel2PayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(ChannelReadyPayload), new ChannelReadyPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(ChannelReestablishPayload),
                         new ChannelReestablishPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(ClosingSignedPayload),
                         new ClosingSignedPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(CommitmentSignedPayload),
                         new CommitmentSignedPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(ErrorPayload), new ErrorPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(InitPayload), new InitPayloadSerializer(_featureSetSerializer));
        _serializers.Add(typeof(OpenChannel2Payload), new OpenChannel2PayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(PingPayload), new PingPayloadSerializer());
        _serializers.Add(typeof(PongPayload), new PongPayloadSerializer());
        _serializers.Add(typeof(RevokeAndAckPayload), new RevokeAndAckPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(ShutdownPayload), new ShutdownPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(StfuPayload), new StfuPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(TxAbortPayload), new TxAbortPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(TxAckRbfPayload), new TxAckRbfPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(TxAddInputPayload), new TxAddInputPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(TxAddOutputPayload), new TxAddOutputPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(TxCompletePayload), new TxCompletePayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(TxInitRbfPayload), new TxInitRbfPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(TxRemoveInputPayload),
                         new TxRemoveInputPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(TxRemoveOutputPayload),
                         new TxRemoveOutputPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(TxSignaturesPayload), new TxSignaturesPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(UpdateAddHtlcPayload), 
                         new UpdateAddHtlcPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(UpdateFailHtlcPayload),
                         new UpdateFailHtlcPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(UpdateFailMalformedHtlcPayload),
                         new UpdateFailMalformedHtlcPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(UpdateFeePayload), new UpdateFeePayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(UpdateFulfillHtlcPayload),
                         new UpdateFulfillHtlcPayloadSerializer(_valueObjectSerializerFactory));
    }

    private void RegisterTypeDictionary()
    {
        _ushortTypeDictionary.Add(MessageTypes.ACCEPT_CHANNEL_2, typeof(AcceptChannel2Payload));
        _ushortTypeDictionary.Add(MessageTypes.CHANNEL_READY, typeof(ChannelReadyPayload));
        _ushortTypeDictionary.Add(MessageTypes.CHANNEL_REESTABLISH, typeof(ChannelReestablishPayload));
        _ushortTypeDictionary.Add(MessageTypes.CLOSING_SIGNED, typeof(ClosingSignedPayload));
        _ushortTypeDictionary.Add(MessageTypes.COMMITMENT_SIGNED, typeof(CommitmentSignedPayload));
        _ushortTypeDictionary.Add(MessageTypes.ERROR, typeof(ErrorPayload));
        _ushortTypeDictionary.Add(MessageTypes.INIT, typeof(InitPayload));
        _ushortTypeDictionary.Add(MessageTypes.OPEN_CHANNEL_2, typeof(OpenChannel2Payload));
        _ushortTypeDictionary.Add(MessageTypes.PING, typeof(PingPayload));
        _ushortTypeDictionary.Add(MessageTypes.PONG, typeof(PongPayload));
        _ushortTypeDictionary.Add(MessageTypes.REVOKE_AND_ACK, typeof(RevokeAndAckPayload));
        _ushortTypeDictionary.Add(MessageTypes.SHUTDOWN, typeof(ShutdownPayload));
        _ushortTypeDictionary.Add(MessageTypes.STFU, typeof(StfuPayload));
        _ushortTypeDictionary.Add(MessageTypes.TX_ABORT, typeof(TxAbortPayload));
        _ushortTypeDictionary.Add(MessageTypes.TX_ACK_RBF, typeof(TxAckRbfPayload));
        _ushortTypeDictionary.Add(MessageTypes.TX_ADD_INPUT, typeof(TxAddInputPayload));
        _ushortTypeDictionary.Add(MessageTypes.TX_ADD_OUTPUT, typeof(TxAddOutputPayload));
        _ushortTypeDictionary.Add(MessageTypes.TX_COMPLETE, typeof(TxCompletePayload));
        _ushortTypeDictionary.Add(MessageTypes.TX_INIT_RBF, typeof(TxInitRbfPayload));
        _ushortTypeDictionary.Add(MessageTypes.TX_REMOVE_INPUT, typeof(TxRemoveInputPayload));
        _ushortTypeDictionary.Add(MessageTypes.TX_REMOVE_OUTPUT, typeof(TxRemoveOutputPayload));
        _ushortTypeDictionary.Add(MessageTypes.TX_SIGNATURES, typeof(TxSignaturesPayload));
        _ushortTypeDictionary.Add(MessageTypes.UPDATE_ADD_HTLC, typeof(UpdateAddHtlcPayload));
        _ushortTypeDictionary.Add(MessageTypes.UPDATE_FAIL_HTLC, typeof(UpdateFailHtlcPayload));
        _ushortTypeDictionary.Add(MessageTypes.UPDATE_FAIL_MALFORMED_HTLC, typeof(UpdateFailMalformedHtlcPayload));
        _ushortTypeDictionary.Add(MessageTypes.UPDATE_FEE, typeof(UpdateFeePayload));
        _ushortTypeDictionary.Add(MessageTypes.UPDATE_FULFILL_HTLC, typeof(UpdateFulfillHtlcPayload));
        _ushortTypeDictionary.Add(MessageTypes.WARNING, typeof(ErrorPayload));
    }
}