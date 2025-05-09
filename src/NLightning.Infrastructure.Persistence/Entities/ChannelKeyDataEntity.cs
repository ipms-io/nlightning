namespace NLightning.Infrastructure.Persistence.Entities;

public class ChannelKeyDataEntity
{
    public Guid Id { get; set; }
    public byte[] ChannelId { get; set; } = [];
    public uint KeyIndex { get; set; }
    public byte[] FundingPubKey { get; set; } = [];
    public byte[] RevocationBasepoint { get; set; } = [];
    public byte[] PaymentBasepoint { get; set; } = [];
    public byte[] DelayedPaymentBasepoint { get; set; } = [];
    public byte[] HtlcBasepoint { get; set; } = [];
}