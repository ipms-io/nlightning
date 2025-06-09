namespace NLightning.Infrastructure.Bitcoin.Options;

public class BitcoinOptions
{
    public required string RpcEndpoint { get; set; }
    public required string RpcUser { get; set; }
    public required string RpcPassword { get; set; }
    public required string ZmqHost { get; set; }
    public required int ZmqBlockPort { get; set; }
    public required int ZmqTxPort { get; set; }
}