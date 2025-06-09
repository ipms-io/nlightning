using System.Buffers;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Interfaces;
using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.ValueObjects;

public class ChannelFlagTypeSerializer : IValueObjectTypeSerializer<ChannelFlags>
{
    public async Task SerializeAsync(IValueObject valueObject, Stream stream)
    {
        if (valueObject is not ChannelFlags channelFlags)
            throw new ArgumentException("Value object must be of type ChannelFlags.", nameof(valueObject));

        await stream.WriteAsync(new ReadOnlyMemory<byte>([channelFlags]));
    }

    public async Task<ChannelFlags> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(1);

        try
        {
            await stream.ReadExactlyAsync(buffer.AsMemory()[..1]);
            return new ChannelFlags(buffer[0]);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    async Task<IValueObject> IValueObjectTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}