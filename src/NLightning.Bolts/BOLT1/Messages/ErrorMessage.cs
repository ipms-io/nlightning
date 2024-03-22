// namespace NLightning.Bolts.BOLT1.Messages;

// public sealed class ErrorMessage(ChannelId channelId, byte[] data) : BaseMessage
// {
//     public static new byte Type => 17;

//     public ErrorData Data { get; } = new ErrorData(channelId, data);

//     public override byte[] Serialize()
//     {
//         using var stream = new MemoryStream();
//         using var writer = new BinaryWriter(stream);

//         writer.Write(Type);
//         writer.Write(Data.ChannelId);
//         writer.Write(Data.Len);
//         writer.Write(Data.Data);

//         return stream.ToArray();
//     }
// }