namespace NLightning.Bolts.BOLT1.Services;

using Bolts.Interfaces;
using Factories;
using Interfaces;

public sealed class MessageService() : IMessageService
{
    public Task<IMessage> ReceiveMessageAsync(Stream networkStream)
    {
        return MessageFactory.DeserializeMessageAsync(networkStream);
    }

    public async Task SendMessageAsync(IMessage message, Stream networkStream)
    {
        await message.SerializeAsync(networkStream);

        await networkStream.FlushAsync();
    }
}