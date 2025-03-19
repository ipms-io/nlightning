using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT3.Integration;

using Common.Types;

public static class AppendixCVectors
{
    public static readonly Key NODE_A_FUNDING_PRIVKEY = new(Convert.FromHexString("30ff4956bbdd3222d44cc5e8a1261dab1e07957bdac5ae88fe3261ef321f3749"));
    public static readonly PubKey NODE_A_FUNDING_PUBKEY = new("023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb");
    public static readonly PubKey NODE_A_PAYMENT_BASEPOINT = new("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa");
    public static readonly PubKey NODE_A_HTLC_BASEPOINT = NODE_A_PAYMENT_BASEPOINT;
    public static readonly Key NODE_A_PRIVKEY = new(Convert.FromHexString("bb13b121cdc357cd2e608b0aea294afca36e2b34cf958e2e6451a2f274694491"));
    public static readonly PubKey NODE_A_HTLC_PUBKEY = new("030d417a46946384f88d5f3337267c5e579765875dc4daca813e21734b140639e7");
    public static readonly PubKey NODE_A_DELAYED_PUBKEY = new("03fd5960528dc152014952efdb702a88f71e3c1653b2314431701ec77e57fde83c");
    public static readonly PubKey NODE_A_REVOCATION_PUBKEY = new("0212a140cd0c6539d07cd08dfe09984dec3251ea808b892efeac3ede9402bf2b19");

    public static readonly PubKey NODE_B_PAYMENT_BASEPOINT = new("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991");
    public static readonly PubKey NODE_B_HTLC_BASEPOINT = NODE_B_PAYMENT_BASEPOINT;
    public static readonly PubKey NODE_B_FUNDING_PUBKEY = new("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991");
    public static readonly PubKey NODE_B_HTLC_PUBKEY = new("0394854aa6eab5b2a8122cc726e9dded053a2184d88256816826d6231c068d4a5b");

    public static readonly LightningMoney TO_LOCAL_MSAT = 7000000000UL;
    public static readonly LightningMoney TO_REMOTE_MSAT = 3000000000UL;

    public const ulong COMMITMENT_NUMBER = 42;
    public const uint LOCAL_DELAY = 144;
    public const ulong LOCAL_DUST_LIMIT_SATOSHI = 546;

    public const ulong EXPECTED_OBSCURING_FACTOR = 0x2bb038521914;
    public static readonly byte[] EXPECTED_TO_LOCAL_WIT_SCRIPT_1 = Convert.FromHexString("63210212a140cd0c6539d07cd08dfe09984dec3251ea808b892efeac3ede9402bf2b1967029000b2752103fd5960528dc152014952efdb702a88f71e3c1653b2314431701ec77e57fde83c68ac");
    public static readonly Script EXPECTED_TO_REMOTE_SCRIPT_1 = new("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991");
    public static readonly Transaction EXPECTED_COMMIT_TX_1 = Transaction.Parse("02000000000101bef67e4e2fb9ddeeb3461973cd4c62abb35050b1add772995b820b584a488489000000000038b02b8002c0c62d0000000000160014cc1b07838e387deacd0e5232e1e8b49f4c29e48454a56a00000000002200204adb4e2f00643db396dd120d4e7dc17625f5f2c11a40d857accc862d6b7dd80e04004730440220616210b2cc4d3afb601013c373bbd8aac54febd9f15400379a8cb65ce7deca60022034236c010991beb7ff770510561ae8dc885b8d38d1947248c38f2ae05564714201483045022100c3127b33dcc741dd6b05b1e63cbd1a9a7d816f37af9b6756fa2376b056f032370220408b96279808fe57eb7e463710804cdf4f108388bc5cf722d8c848d2c7f9f3b001475221023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb21030e9f7b623d2ccc7c9bd44d66d5ce21ce504c0acf6385a132cec6d3c39fa711c152ae3e195220", Network.MAIN_NET);
}