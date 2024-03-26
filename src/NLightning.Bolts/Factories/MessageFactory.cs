namespace NLightning.Bolts.Factories;

using BOLT1.Types;
using Interfaces;
using Messages;
using NLightning.Bolts.Constants;

public static class MessageFactory
{
    public static IMessage DeserializeMessage(BinaryReader reader)
    {
        // Get type of message
        var type = EndianBitConverter.ToUInt16BE(reader.ReadBytes(2));

        // Deserialize payload based on type
        IMessagePayload payload = type switch
        {
            MessageTypes.INIT => new InitPayload(reader),
            _ => throw new Exception("Unknown payload type"),
        };

        // Deserialize extension if available
        if (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var extension = new TLVStream(reader);
            return new Message(type, payload, extension);
        }

        return new Message(type, payload);
    }
}