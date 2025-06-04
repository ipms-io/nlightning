namespace NLightning.Tests.Utils.Vectors;

public static class Bolt3AppendixDVectors
{
    public static readonly byte[] Seed0FinalNode =
        Convert.FromHexString("0000000000000000000000000000000000000000000000000000000000000000");

    public static readonly byte[] SeedFfFinalNode =
        Convert.FromHexString("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

    public static readonly byte[] SeedFfAlternateBits1 =
        Convert.FromHexString("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

    public static readonly byte[] SeedFfAlternateBits2 =
        Convert.FromHexString("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

    public static readonly byte[] Seed01LastNonTrivialNode =
        Convert.FromHexString("0101010101010101010101010101010101010101010101010101010101010101");

    public const ulong I0FinalNode = 281474976710655;
    public const ulong IFfFinalNode = 281474976710655;
    public const ulong IFfAlternateBits1 = 0xaaaaaaaaaaa;
    public const ulong IFfAlternateBits2 = 0x555555555555;
    public const ulong I01LastNonTrivialNode = 1;

    public static readonly byte[] ExpectedOutput0FinalNode =
        Convert.FromHexString("02a40c85b6f28da08dfdbe0926c53fab2de6d28c10301f8f7c4073d5e42e3148");

    public static readonly byte[] ExpectedOutputFfFinalNode =
        Convert.FromHexString("7cc854b54e3e0dcdb010d7a3fee464a9687be6e8db3be6854c475621e007a5dc");

    public static readonly byte[] ExpectedOutputFfAlternateBits1 =
        Convert.FromHexString("56f4008fb007ca9acf0e15b054d5c9fd12ee06cea347914ddbaed70d1c13a528");

    public static readonly byte[] ExpectedOutputFfAlternateBits2 =
        Convert.FromHexString("9015daaeb06dba4ccc05b91b2f73bd54405f2be9f217fbacd3c5ac2e62327d31");

    public static readonly byte[] ExpectedOutput01LastNonTrivialNode =
        Convert.FromHexString("915c75942a26bb3a433a8ce2cb0427c29ec6c1775cfc78328b57f6ba7bfeaa9c");

    public const ulong StorageIndexMax = 0xFFFFFFFFFFFFUL;

    public static readonly byte[] StorageCorrectSeed =
        Convert.FromHexString("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

    public static readonly byte[] StorageIncorrectSeed =
        Convert.FromHexString("0000000000000000000000000000000000000000000000000000000000000000");

    public static readonly byte[] StorageExpectedSecret0 =
        Convert.FromHexString("7cc854b54e3e0dcdb010d7a3fee464a9687be6e8db3be6854c475621e007a5dc");

    public static readonly byte[] StorageExpectedSecret1 =
        Convert.FromHexString("c7518c8ae4660ed02894df8976fa1a3659c1a8b4b5bec0c4b872abeba4cb8964");

    public static readonly byte[] StorageExpectedSecret2 =
        Convert.FromHexString("2273e227a5b7449b6e70f1fb4652864038b1cbf9cd7c043a7d6456b7fc275ad8");

    public static readonly byte[] StorageExpectedSecret3 =
        Convert.FromHexString("27cddaa5624534cb6cb9d7da077cf2b22ab21e9b506fd4998a51d54502e99116");

    public static readonly byte[] StorageExpectedSecret4 =
        Convert.FromHexString("c65716add7aa98ba7acb236352d665cab17345fe45b55fb879ff80e6bd0c41dd");

    public static readonly byte[] StorageExpectedSecret5 =
        Convert.FromHexString("969660042a28f32d9be17344e09374b379962d03db1574df5a8a5a47e19ce3f2");

    public static readonly byte[] StorageExpectedSecret6 =
        Convert.FromHexString("a5a64476122ca0925fb344bdc1854c1c0a59fc614298e50a33e331980a220f32");

    public static readonly byte[] StorageExpectedSecret7 =
        Convert.FromHexString("05cde6323d949933f7f7b78776bcc1ea6d9b31447732e3802e1f7ac44b650e17");

    public static readonly byte[] StorageExpectedSecret8 =
        Convert.FromHexString("02a40c85b6f28da08dfdbe0926c53fab2de6d28c10301f8f7c4073d5e42e3148");

    public static readonly byte[] StorageExpectedSecret9 =
        Convert.FromHexString("dddc3a8d14fddf2b68fa8c7fbad2748274937479dd0f8930d5ebb4ab6bd866a3");

    public static readonly byte[] StorageExpectedSecret10 =
        Convert.FromHexString("c51a18b13e8527e579ec56365482c62f180b7d5760b46e9477dae59e87ed423a");

    public static readonly byte[] StorageExpectedSecret11 =
        Convert.FromHexString("ba65d7b0ef55a3ba300d4e87af29868f394f8f138d78a7011669c79b37b936f4");

    public static readonly byte[] StorageExpectedSecret12 =
        Convert.FromHexString("631373ad5f9ef654bb3dade742d09504c567edd24320d2fcd68e3cc47e2ff6a6");

    public static readonly byte[] StorageExpectedSecret13 =
        Convert.FromHexString("b7e76a83668bde38b373970155c868a653304308f9896692f904a23731224bb1");

    public static readonly byte[] StorageExpectedSecret14 =
        Convert.FromHexString("e7971de736e01da8ed58b94c2fc216cb1dca9e326f3a96e7194fe8ea8af6c0a3");

    public static readonly byte[] StorageExpectedSecret15 =
        Convert.FromHexString("a7efbc61aac46d34f77778bac22c8a20c6a46ca460addc49009bda875ec88fa4");
}