using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT3.Integration;

public static class AppendixCVectors
{
    public static readonly Key NODE_A_FUNDING_PRIVKEY = new(Convert.FromHexString("30ff4956bbdd3222d44cc5e8a1261dab1e07957bdac5ae88fe3261ef321f374901"));
    public static readonly PubKey NODE_A_FUNDING_PUBKEY = new("023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb");
    public static readonly PubKey NODE_A_PAYMENT_BASEPOINT = new("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa");
    public static readonly PubKey NODE_A_HTLC_BASEPOINT = NODE_A_PAYMENT_BASEPOINT;
    public static readonly Key NODE_A_PRIVKEY = new(Convert.FromHexString("bb13b121cdc357cd2e608b0aea294afca36e2b34cf958e2e6451a2f27469449101"));
    public static readonly PubKey NODE_A_HTLC_PUBKEY = new("030d417a46946384f88d5f3337267c5e579765875dc4daca813e21734b140639e7");
    public static readonly PubKey NODE_A_DELAYED_PUBKEY = new("03fd5960528dc152014952efdb702a88f71e3c1653b2314431701ec77e57fde83c");
    public static readonly PubKey NODE_A_REVOCATION_PUBKEY = new("0212a140cd0c6539d07cd08dfe09984dec3251ea808b892efeac3ede9402bf2b19");

    public static readonly PubKey NODE_B_PAYMENT_BASEPOINT = new("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991");
    public static readonly PubKey NODE_B_HTLC_BASEPOINT = NODE_B_PAYMENT_BASEPOINT;
    public static readonly PubKey NODE_B_FUNDING_PUBKEY = new("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991");
    public static readonly PubKey NODE_B_HTLC_PUBKEY = new("0394854aa6eab5b2a8122cc726e9dded053a2184d88256816826d6231c068d4a5b");

    public const ulong TO_LOCAL_MSAT = 7000000000;
    public const ulong TO_REMOTE_MSAT = 3000000000;
    public const ulong COMMITMENT_NUMBER = 42;
    public const uint LOCAL_DELAY = 144;
    public const ulong LOCAL_DUST_LIMIT_SATOSHI = 546;
}