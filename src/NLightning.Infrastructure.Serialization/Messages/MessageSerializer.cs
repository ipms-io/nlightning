namespace NLightning.Infrastructure.Serialization.Messages;

using Converters;
using Domain.Protocol.Messages.Interfaces;
using Domain.Serialization.Factories;
using Domain.Serialization.Messages;
using Exceptions;

public class MessageSerializer : IMessageSerializer
{
    private readonly IMessageTypeSerializerFactory _messageTypeSerializerFactory;

    public MessageSerializer(IMessageTypeSerializerFactory messageTypeSerializerFactory)
    {
        _messageTypeSerializerFactory = messageTypeSerializerFactory;
    }

    public async Task SerializeAsync(IMessage message, Stream stream)
    {
        var messageTypeSerializer = _messageTypeSerializerFactory.GetSerializer(message.Type) ?? throw new InvalidOperationException($"No serializer found for message type {message.Type}");

        // Write the message type to the stream
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(message.Type));

        // Serialize the message
        await messageTypeSerializer.SerializeAsync(message, stream);
    }

    public async Task<TMessage?> DeserializeMessageAsync<TMessage>(Stream stream) where TMessage : class, IMessage
    {
        // Get the type of the message
        var typeBytes = new byte[2];
        await stream.ReadExactlyAsync(typeBytes);
        var type = EndianBitConverter.ToUInt16BigEndian(typeBytes);

        // Try to get the serializer for the message type
        var messageTypeSerializer = _messageTypeSerializerFactory.GetSerializer<TMessage>();
        if (messageTypeSerializer is not null)
            return await messageTypeSerializer.DeserializeAsync(stream);

        // If the type is unknown and even, throw an exception
        if (type % 2 == 0)
        {
            throw new InvalidMessageException($"Unknown message type {type}");
        }

        return null;

        // case MessageTypes.OPEN_CHANNEL:
        // case MessageTypes.ACCEPT_CHANNEL:
        // case MessageTypes.FUNDING_CREATED:
        // case MessageTypes.FUNDING_SIGNED:
        //     throw new InvalidMessageException("You must use OpenChannel2 flow");
    }

    public async Task<IMessage?> DeserializeMessageAsync(Stream stream)
    {
        // Get the type of the message
        var typeBytes = new byte[2];
        await stream.ReadExactlyAsync(typeBytes);
        var type = EndianBitConverter.ToUInt16BigEndian(typeBytes);

        // Try to get the serializer for the message type
        var messageTypeSerializer = _messageTypeSerializerFactory.GetSerializer(type);
        if (messageTypeSerializer is not null)
            return await messageTypeSerializer.DeserializeAsync(stream);

        // If the type is unknown and even, throw an exception
        if (type % 2 == 0)
            throw new InvalidMessageException($"Unknown message type {type}");

        return null;
    }
}