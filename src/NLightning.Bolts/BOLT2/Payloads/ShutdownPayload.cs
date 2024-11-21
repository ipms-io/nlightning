using NBitcoin;

namespace NLightning.Bolts.BOLT2.Payloads;

using Common.BitUtils;
using Exceptions;
using Interfaces;

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
    public ChannelId ChannelId { get; set; } = channelId;

    /// <summary>
    /// len is the scriptpubkey length
    /// </summary>
    public ushort ScriptPubkeyLen { get; set; } = (ushort)scriptPubkey.Length;

    /// <summary>
    /// The scriptpubkey to send the closing funds to
    /// </summary>
    public Script ScriptPubkey { get; set; } = scriptPubkey;

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

            var bytes = new byte[sizeof(ushort)];
            await stream.ReadExactlyAsync(bytes);
            var len = EndianBitConverter.ToUInt16BigEndian(bytes);

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