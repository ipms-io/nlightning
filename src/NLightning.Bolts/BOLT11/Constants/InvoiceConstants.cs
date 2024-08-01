namespace NLightning.Bolts.BOLT11.Constants;

public static class InvoiceConstants
{
    public const string PREFIX = "ln";
    public const char SEPARATOR = '1';
    public const string PREFIX_MAINET = "bc";
    public const string PREFIX_TESTNET = "tb";
    public const string PREFIX_SIGNET = "tbs";
    public const string PREFIX_REGTEST = "bcrt";
    public const char MULTIPLIER_MILLI = 'm';
    public const char MULTIPLIER_MICRO = 'u';
    public const char MULTIPLIER_NANO = 'n';
    public const char MULTIPLIER_PICO = 'p';
    public const decimal BTC_IN_SATOSHIS = 100_000_000m;
    public const decimal BTC_IN_MILLISATOSHIS = 100_000_000_000m;
    public const int DEFAULT_EXPIRATION_SECONDS = 3600;
}