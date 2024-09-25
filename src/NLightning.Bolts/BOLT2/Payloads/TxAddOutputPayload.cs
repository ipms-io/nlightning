using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Payloads;

using Common.BitUtils;
using Common.Types;
using Interfaces;

/// <summary>
/// Represents a tx_add_output payload.
/// </summary>
/// <remarks>
/// The tx_add_output payload is used to add an output to the transaction.
/// </remarks>
/// <seealso cref="Messages.TxAddOutputMessage"/>
/// <seealso cref="Common.Types.ChannelId"/>
public class TxAddOutputPayload : IMessagePayload
{
    /// <summary>
    /// The channel id.
    /// </summary>
    public ChannelId ChannelId { get; }

    /// <summary>
    /// The serial id.
    /// </summary>
    public ulong SerialId { get; }

    /// <summary>
    /// The sats amount.
    /// </summary>
    public ulong Sats { get; }

    /// <summary>
    /// The spending script.
    /// </summary>
    public byte[] Script { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TxAddOutputPayload"/> class.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="serialId">The serial id.</param>
    /// <param name="sats">The sats amount.</param>
    /// <param name="script">The spending script.</param>
    /// <exception cref="ArgumentException">Script length is out of bounds.</exception>
    public TxAddOutputPayload(ChannelId channelId, ulong serialId, ulong sats, byte[] script)
    {
        if (script.Length > ushort.MaxValue)
        {
            throw new ArgumentException($"Script length must be less than or equal to {ushort.MaxValue} bytes", nameof(script));
        }

        ChannelId = channelId;
        SerialId = serialId;
        Sats = sats;
        Script = script;
    }

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(SerialId));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(Sats));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian((ushort)Script.Length));
        await stream.WriteAsync(Script);
    }

    /// <summary>
    /// Deserialize a TxAddOutputPayload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxAddOutputPayload.</returns>
    /// <exception cref="SerializationException">Error deserializing Payload</exception>
    public static async Task<TxAddOutputPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var bytes = new byte[8];
            await stream.ReadExactlyAsync(bytes);
            var serialId = EndianBitConverter.ToUInt64BigEndian(bytes);

            bytes = new byte[8];
            await stream.ReadExactlyAsync(bytes);
            var sats = EndianBitConverter.ToUInt64BigEndian(bytes);

            bytes = new byte[2];
            await stream.ReadExactlyAsync(bytes);
            var scriptLength = EndianBitConverter.ToUInt16BigEndian(bytes);

            var script = new byte[scriptLength];
            await stream.ReadExactlyAsync(script);

            return new TxAddOutputPayload(channelId, serialId, sats, script);
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing TxAddOutputPayload", e);
        }
    }
}