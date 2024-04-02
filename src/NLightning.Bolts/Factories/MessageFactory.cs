namespace NLightning.Bolts.Factories;

using BOLT1.Messages;
using BOLT1.Payloads;
using Bolts.Constants;
using Interfaces;

public static class MessageFactory
{
    #region Init Message
    public static IMessage CreateInitMessage(NodeOptions options)
    {
        // Get features from options
        var features = options.GetNodeFeatures();
        var payload = new InitPayload(features);

        // Add default extension for Init message from options
        var extension = options.GetInitExtension();

        return new InitMessage(payload, extension);
    }
    #endregion

    #region Control Messages
    public static IMessage CreatePingMessage()
    {
        return new PingMessage();
    }

    public static IMessage CreatePongMessage(ushort bytesLen)
    {
        return new PongMessage(bytesLen);
    }
    #endregion

    public static async Task<IMessage> DeserializeMessageAsync(MemoryStream stream)
    {
        // Get type of message
        var typeBytes = new byte[2];
        await stream.ReadExactlyAsync(typeBytes);
        var type = EndianBitConverter.ToUInt16BE(typeBytes);

        // Deserialize message based on type
        return type switch
        {
            MessageTypes.WARNING => await WarningMessage.DeserializeAsync(stream),
            MessageTypes.INIT => await InitMessage.DeserializeAsync(stream),
            MessageTypes.ERROR => await ErrorMessage.DeserializeAsync(stream),
            MessageTypes.PING => await PingMessage.DeserializeAsync(stream),
            MessageTypes.PONG => await PongMessage.DeserializeAsync(stream),

            _ => throw new Exception("Unknown payload type"),
        };
    }
}