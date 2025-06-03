using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Factories;

using Domain.Protocol.Constants;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Interfaces;
using Payloads;

public class PayloadSerializerFactory : IPayloadSerializerFactory
{
    private readonly IFeatureSetSerializer _featureSetSerializer;
    private readonly Dictionary<Type, IPayloadSerializer> _serializers = new();
    private readonly Dictionary<MessageTypes, Type> _messageTypeDictionary = new();
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

    public IPayloadSerializer? GetSerializer(MessageTypes messageType)
    {
        var type = _messageTypeDictionary.GetValueOrDefault(messageType);
        return type is null ? null : _serializers.GetValueOrDefault(type);
    }

    private void RegisterSerializers()
    {
        _serializers.Add(typeof(AcceptChannel1Payload),
                         new AcceptChannel1PayloadSerializer(_valueObjectSerializerFactory));
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
        _serializers.Add(typeof(FundingCreatedPayload),
                         new FundingCreatedPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(FundingSignedPayload),
                         new FundingSignedPayloadSerializer(_valueObjectSerializerFactory));
        _serializers.Add(typeof(InitPayload), new InitPayloadSerializer(_featureSetSerializer));
        _serializers.Add(typeof(OpenChannel1Payload), new OpenChannel1PayloadSerializer(_valueObjectSerializerFactory));
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
        _messageTypeDictionary.Add(MessageTypes.AcceptChannel, typeof(AcceptChannel1Payload));
        _messageTypeDictionary.Add(MessageTypes.AcceptChannel2, typeof(AcceptChannel2Payload));
        _messageTypeDictionary.Add(MessageTypes.ChannelReady, typeof(ChannelReadyPayload));
        _messageTypeDictionary.Add(MessageTypes.ChannelReestablish, typeof(ChannelReestablishPayload));
        _messageTypeDictionary.Add(MessageTypes.ClosingSigned, typeof(ClosingSignedPayload));
        _messageTypeDictionary.Add(MessageTypes.CommitmentSigned, typeof(CommitmentSignedPayload));
        _messageTypeDictionary.Add(MessageTypes.Error, typeof(ErrorPayload));
        _messageTypeDictionary.Add(MessageTypes.FundingCreated, typeof(FundingCreatedPayload));
        _messageTypeDictionary.Add(MessageTypes.FundingSigned, typeof(FundingSignedPayload));
        _messageTypeDictionary.Add(MessageTypes.Init, typeof(InitPayload));
        _messageTypeDictionary.Add(MessageTypes.OpenChannel, typeof(OpenChannel1Payload));
        _messageTypeDictionary.Add(MessageTypes.OpenChannel2, typeof(OpenChannel2Payload));
        _messageTypeDictionary.Add(MessageTypes.Ping, typeof(PingPayload));
        _messageTypeDictionary.Add(MessageTypes.Pong, typeof(PongPayload));
        _messageTypeDictionary.Add(MessageTypes.RevokeAndAck, typeof(RevokeAndAckPayload));
        _messageTypeDictionary.Add(MessageTypes.Shutdown, typeof(ShutdownPayload));
        _messageTypeDictionary.Add(MessageTypes.Stfu, typeof(StfuPayload));
        _messageTypeDictionary.Add(MessageTypes.TxAbort, typeof(TxAbortPayload));
        _messageTypeDictionary.Add(MessageTypes.TxAckRbf, typeof(TxAckRbfPayload));
        _messageTypeDictionary.Add(MessageTypes.TxAddInput, typeof(TxAddInputPayload));
        _messageTypeDictionary.Add(MessageTypes.TxAddOutput, typeof(TxAddOutputPayload));
        _messageTypeDictionary.Add(MessageTypes.TxComplete, typeof(TxCompletePayload));
        _messageTypeDictionary.Add(MessageTypes.TxInitRbf, typeof(TxInitRbfPayload));
        _messageTypeDictionary.Add(MessageTypes.TxRemoveInput, typeof(TxRemoveInputPayload));
        _messageTypeDictionary.Add(MessageTypes.TxRemoveOutput, typeof(TxRemoveOutputPayload));
        _messageTypeDictionary.Add(MessageTypes.TxSignatures, typeof(TxSignaturesPayload));
        _messageTypeDictionary.Add(MessageTypes.UpdateAddHtlc, typeof(UpdateAddHtlcPayload));
        _messageTypeDictionary.Add(MessageTypes.UpdateFailHtlc, typeof(UpdateFailHtlcPayload));
        _messageTypeDictionary.Add(MessageTypes.UpdateFailMalformedHtlc, typeof(UpdateFailMalformedHtlcPayload));
        _messageTypeDictionary.Add(MessageTypes.UpdateFee, typeof(UpdateFeePayload));
        _messageTypeDictionary.Add(MessageTypes.UpdateFulfillHtlc, typeof(UpdateFulfillHtlcPayload));
        _messageTypeDictionary.Add(MessageTypes.Warning, typeof(ErrorPayload));
    }
}