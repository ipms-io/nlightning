using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Payloads;

using Common.BitUtils;
using Interfaces;

/// <summary>
/// Represents the payload for the tx_init_rbf message.
/// </summary>
/// <remarks>
/// The tx_init_rbf message is used to initialize a new RBF transaction.
/// </remarks>
/// 
/// <remarks>
/// Initializes a new instance of the <see cref="TxInitRbfPayload"/> class.
/// </remarks>
/// <param name="channelId">The channel ID.</param>
/// <param name="locktime">The locktime.</param>
/// <param name="feerate">The feerate.</param>
/// <seealso cref="Messages.TxInitRbfMessage"/>
/// <seealso cref="Common.Types.ChannelId"/>
/// <seealso cref="Common.Types.TLVStream"/>
public class TxInitRbfPayload(ChannelId channelId, uint locktime, uint feerate) : IMessagePayload
{
    /// <summary>
    /// The channel ID.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// The locktime.
    /// </summary>
    public uint Locktime { get; } = locktime;

    /// <summary>
    /// The feerate.
    /// </summary>
    public uint Feerate { get; } = feerate;

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(Locktime));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(Feerate));
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="SerializationException">Error deserializing Payload</exception>
    public static async Task<TxInitRbfPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var bytes = new byte[4];
            await stream.ReadExactlyAsync(bytes);
            var locktime = EndianBitConverter.ToUInt32BigEndian(bytes);

            bytes = new byte[4];
            await stream.ReadExactlyAsync(bytes);
            var feerate = EndianBitConverter.ToUInt32BigEndian(bytes);

            return new TxInitRbfPayload(channelId, locktime, feerate);
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing TxInitRbfPayload", e);
        }
    }
}