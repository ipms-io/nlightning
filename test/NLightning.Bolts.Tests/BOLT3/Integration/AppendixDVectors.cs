namespace NLightning.Bolts.Tests.BOLT3.Integration;
public static class AppendixDVectors
{
    public static readonly byte[] SEED_0_FINAL_NODE = Convert.FromHexString("0000000000000000000000000000000000000000000000000000000000000000");
    public static readonly byte[] SEED_FF_FINAL_NODE = Convert.FromHexString("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
    public static readonly byte[] SEED_FF_ALTERNATE_BITS_1 = Convert.FromHexString("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
    public static readonly byte[] SEED_FF_ALTERNATE_BITS_2 = Convert.FromHexString("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
    public static readonly byte[] SEED_01_LAST_NON_TRIVIAL_NODE = Convert.FromHexString("0101010101010101010101010101010101010101010101010101010101010101");

    public const ulong I_0_FINAL_NODE = 281474976710655;
    public const ulong I_FF_FINAL_NODE = 281474976710655;
    public const ulong I_FF_ALTERNATE_BITS_1 = 0xaaaaaaaaaaa;
    public const ulong I_FF_ALTERNATE_BITS_2 = 0x555555555555;
    public const ulong I_01_LAST_NON_TRIVIAL_NODE = 1;

    public static readonly byte[] EXPECTED_OUTPUT_0_FINAL_NODE = Convert.FromHexString("02a40c85b6f28da08dfdbe0926c53fab2de6d28c10301f8f7c4073d5e42e3148");
    public static readonly byte[] EXPECTED_OUTPUT_FF_FINAL_NODE = Convert.FromHexString("7cc854b54e3e0dcdb010d7a3fee464a9687be6e8db3be6854c475621e007a5dc");
    public static readonly byte[] EXPECTED_OUTPUT_FF_ALTERNATE_BITS_1 = Convert.FromHexString("56f4008fb007ca9acf0e15b054d5c9fd12ee06cea347914ddbaed70d1c13a528");
    public static readonly byte[] EXPECTED_OUTPUT_FF_ALTERNATE_BITS_2 = Convert.FromHexString("9015daaeb06dba4ccc05b91b2f73bd54405f2be9f217fbacd3c5ac2e62327d31");
    public static readonly byte[] EXPECTED_OUTPUT_01_LAST_NON_TRIVIAL_NODE = Convert.FromHexString("915c75942a26bb3a433a8ce2cb0427c29ec6c1775cfc78328b57f6ba7bfeaa9c");
}