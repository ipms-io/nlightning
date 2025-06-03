using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Messages;

using Converters;
using Domain.Protocol.Constants;
using Domain.Protocol.Messages.Interfaces;
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
        var messageTypeSerializer =
            _messageTypeSerializerFactory.GetSerializer(message.Type)
            ?? throw new InvalidOperationException($"No serializer found for message type {message.Type}");

        // Write the message type to the stream
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian((ushort)message.Type));

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
    }

    public async Task<IMessage?> DeserializeMessageAsync(Stream stream)
    {
        // Get the type of the message
        var typeBytes = new byte[2];
        await stream.ReadExactlyAsync(typeBytes);
        var type = EndianBitConverter.ToUInt16BigEndian(typeBytes);

        // Try to get the serializer for the message type
        var messageTypeSerializer = _messageTypeSerializerFactory.GetSerializer((MessageTypes)type);
        if (messageTypeSerializer is not null)
            return await messageTypeSerializer.DeserializeAsync(stream);

        // If the type is unknown and even, throw an exception
        if (type % 2 == 0)
            throw new InvalidMessageException($"Unknown message type {type}");

        return null;
    }
}