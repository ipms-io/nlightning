namespace NLightning.Bolts.BOLT1.Services;

using Bolts.BOLT9;
using Bolts.Constants;
using Bolts.Exceptions;
using Bolts.Messages;
using Interfaces;
using Types;

public sealed class MessageService(Bolt1Options options) : IMessageService
{
    private readonly Bolt1Options _options = options;
    private readonly Features _features = options.GetNodeFeatures();

    #region Init Message
    public Message CreateInitMessage()
    {
        // Create Init Payload with features
        var payload = new InitPayload(_features);

        // Add default extension for Init message from options
        var extension = _options.GetInitExtension();

        return new Message(MessageTypes.INIT, payload, extension);
    }

    public Bolt1Options ValidateInitMessage(Message message)
    {
        // Check message type
        if (message.Type != MessageTypes.INIT)
        {
            throw new InvalidMessageException("Expected {0} message but got {1}", message.Type, MessageTypes.INIT);
        }

        // Check payload
        if (message.Payload is not InitPayload initPayload)
        {
            throw new InvalidMessageException("Expected InitPayload but got {0}", message.Payload.GetType().Name);
        }

        // Check features compatibility
        if (!_features.IsCompatible(initPayload.Features))
        {
            throw new InvalidMessageException("Incompatible features");
        }

        return Bolt1Options.GetBolt1Options(initPayload.Features, message.Extension);
    }
    #endregion
}