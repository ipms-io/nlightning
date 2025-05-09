using NBitcoin;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

/// <summary>
/// Represents the payload for the shutdown message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ShutdownPayload class.
/// </remarks>
public class ShutdownPayload(ChannelId channelId, Script scriptPubkey) : IMessagePayload
{
    /// <summary>
    /// The channel_id this message refers to
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// len is the scriptpubkey length
    /// </summary>
    public ushort ScriptPubkeyLen { get; } = (ushort)scriptPubkey.Length;

    /// <summary>
    /// The scriptpubkey to send the closing funds to
    /// </summary>
    public Script ScriptPubkey { get; } = scriptPubkey;

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(ScriptPubkeyLen));
        await stream.WriteAsync(ScriptPubkey.ToBytes());
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<ShutdownPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var buffer = new byte[sizeof(ushort)];
            await stream.ReadExactlyAsync(buffer);
            var len = EndianBitConverter.ToUInt16BigEndian(buffer);

            var scriptPubkeyBytes = new byte[len];
            await stream.ReadExactlyAsync(scriptPubkeyBytes);
            var scriptPubkey = new Script(scriptPubkeyBytes);

            return new ShutdownPayload(channelId, scriptPubkey);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing ShutdownPayload", e);
        }
    }
}