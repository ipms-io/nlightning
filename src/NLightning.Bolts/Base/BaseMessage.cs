namespace NLightning.Bolts.Base;

using Interfaces;
using BOLT1.Types;
using System.Runtime.Serialization;

/// <summary>
/// Base class with built-in serialization and deserialization for messages
/// </summary>
/// <typeparam name="PayloadType" cref="IMessagePayload">The payload type</typeparam>
public abstract class BaseMessage<PayloadType> : IMessage<PayloadType> where PayloadType : IMessagePayload
{
    public abstract ushort MessageType { get; }
    public abstract PayloadType? Payload { get; set; }
    public abstract TLVStream? Extension { get; set; }
    public abstract Func<BinaryReader, PayloadType> PayloadFactory { get; }

    protected BaseMessage() { }

    /// <summary>
    /// Serialize the message to a writer
    /// </summary>
    public virtual void ToWriter(BinaryWriter writer)
    {
        writer.Write(EndianBitConverter.GetBytesBE(MessageType));
        Payload?.ToWriter(writer);

        if (Extension?.Any() ?? false)
        {
            Extension.Ordered() // Order by type
                     .ToList()  // Convert to list
                     .ForEach(tlv => writer.Write(tlv.Serialize()));
        }
    }

    public virtual byte[] Serialize()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        ToWriter(writer);
        return stream.ToArray();
    }

    /// <summary>
    /// Deserialize the message from a reader
    /// </summary>
    /// <param name="reader">The reader to deserialize from</param>
    /// <exception cref="SerializationException">Thrown when the message type is invalid</exception>
    /// <exception cref="SerializationException">Thrown when the payload cannot be deserialized</exception>
    protected static MessageType FromReader<MessageType>(BinaryReader reader)
        where MessageType : BaseMessage<PayloadType>, new()
    {
        var message = new MessageType();
        // Read the message type
        if (message.MessageType != EndianBitConverter.ToUInt16BE(reader.ReadBytes(2)))
        {
            throw new SerializationException("Invalid message type");
        }

        try
        {
            // Read the payload
            message.Payload = message.PayloadFactory(reader);

            // Read the TLVStream
            message.Extension = TLVStream.Deserialize(reader);

            return message;
        }
        catch (SerializationException e)
        {
            throw new SerializationException("Error deserializing message payload", e);
        }
    }
}