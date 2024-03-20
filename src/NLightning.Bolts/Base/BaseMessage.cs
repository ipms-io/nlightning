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
    public abstract PayloadType? Data { get; }
    public TLVStream? Extension { get; set; }

    /// <summary>
    /// Serialize the message
    /// </summary>
    /// <returns>The serialized message</returns>
    public byte[] Serialize()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write(EndianBitConverter.GetBytesBE(MessageType));
        writer.Write(Data?.Serialize() ?? []);

        if (Extension?.Any() ?? false)
        {
            Extension.Ordered() // Order by type
                     .ToList()  // Convert to list
                     .ForEach(tlv => writer.Write(tlv.Serialize()));
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Deserialize the message
    /// </summary>
    /// <param name="data">The serialized message</param>
    /// <exception cref="SerializationException">Thrown when the message type is invalid</exception>
    /// <exception cref="SerializationException">Thrown when the payload cannot be deserialized</exception>
    public void Deserialize(byte[] data)
    {
        // Create a reader
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        // Read the message type
        var messageType = EndianBitConverter.ToUInt16BE(reader.ReadBytes(2));
        if (messageType != MessageType)
        {
            throw new SerializationException("Invalid message type");
        }

        try
        {
            // Read the payload
            Data?.Deserialize(reader);

            // Read the TLVStream
            Extension = TLVStream.Deserialize(reader);
        }
        catch (SerializationException e)
        {
            throw new SerializationException("Error deserializing message payload", e);
        }
    }
}