using System.Runtime.Serialization;
using NBitcoin;

namespace NLightning.Bolts.BOLT2.Payloads;

using Common.BitUtils;
using Interfaces;

public class ShutdownPayload : IMessagePayload
{
    /// <summary>
    /// The channel_id this message refers to
    /// </summary>
    public ChannelId ChannelId { get; set; }

    /// <summary>
    /// len is the scriptpubkey length
    /// </summary>
    public ushort ScriptPubkeyLen { get; set; }

    /// <summary>
    /// The scriptpubkey to send the closing funds to
    /// </summary>
    public Script ScriptPubkey { get; set; }

    public ShutdownPayload(ChannelId channelId, Script scriptPubkey)
    {
        ChannelId = channelId;
        ScriptPubkeyLen = (ushort)scriptPubkey.Length;
        ScriptPubkey = scriptPubkey;
    }

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
    /// <exception cref="SerializationException">Error deserializing Payload</exception>
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
            throw new SerializationException("Error deserializing ShutdownPayload", e);
        }
    }
}