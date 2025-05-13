using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Factories;

using Domain.Protocol.Constants;
using Domain.Protocol.Interfaces;
using Interfaces;
using Messages;

public class MessageSerializerFactoryFactory : IMessageSerializerFactory
{
    private readonly Dictionary<ushort, IMessageSerializer> _serializers = new();
    private readonly IPayloadSerializerFactory _payloadSerializerFactory;
    
    public MessageSerializerFactoryFactory(IPayloadSerializerFactory payloadSerializerFactory)
    {
        _payloadSerializerFactory = payloadSerializerFactory;
        RegisterSerializers();
    }
    
    public Task SerializeAsync(IMessage message, Stream stream)
    {
        if (_serializers.TryGetValue(message.Type, out var serializer))
        {
            return serializer.SerializeAsync(message, stream);
        }
        
        throw new SerializationException($"No serializer found for message type {message.Type}");
    }

    public Task<TMessage> DeserializeAsync<TMessage>(Stream stream, ushort messageType) where TMessage : IMessage
    {
        if (_serializers.TryGetValue(messageType, out var serializer))
        {
            return serializer.DeserializeAsync(stream) as Task<TMessage>
                   ?? throw new SerializationException($"Deserialized message is not of type {typeof(TMessage)}");
        }
        
        throw new SerializationException($"No serializer found for message type {messageType}");
    }
    
    private void RegisterSerializers()
    {
        _serializers.Add(MessageTypes.ACCEPT_CHANNEL_2, new AcceptChannel2MessageSerializer(_payloadSerializerFactory));
        _serializers.Add(MessageTypes.CHANNEL_READY, new ChannelReadyMessageSerializer(_payloadSerializerFactory));
        _serializers.Add(MessageTypes.CHANNEL_REESTABLISH, new ChannelReestablishMessageSerializer(_payloadSerializerFactory));
        _serializers.Add(MessageTypes.CLOSING_SIGNED, new ClosingSignedMessageSerializer(_payloadSerializerFactory));
        _serializers.Add(MessageTypes.COMMITMENT_SIGNED, new CommitmentSignedMessageSerializer(_payloadSerializerFactory));
        _serializers.Add(MessageTypes.ERROR, new ErrorMessageSerializer(_payloadSerializerFactory));
        _serializers.Add(MessageTypes.INIT, new InitMessageSerializer(_payloadSerializerFactory));
        _serializers.Add(MessageTypes.OPEN_CHANNEL_2, new OpenChannel2MessageSerializer(_payloadSerializerFactory));
        _serializers.Add(MessageTypes.PING, new PingMessageSerializer(_payloadSerializerFactory));
        _serializers.Add(MessageTypes.PONG, new PongMessageSerializer(_payloadSerializerFactory));
        _serializers.Add(MessageTypes.REVOKE_AND_ACK, new RevokeAndAckMessageSerializer(_payloadSerializerFactory));
    }
}