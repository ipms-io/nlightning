namespace NLightning.Bolts.Tests.BOLT8;

using Bolts.BOLT8;

public class UtilitiesTest
{
    private const int ALIGNMENT = 64;

    private static readonly Dictionary<IntPtr, IntPtr> s_tests = new Dictionary<IntPtr, IntPtr>
        {
            {IntPtr.Zero, IntPtr.Zero},
            {1, 64},
            {1023, 1024},
            {unchecked((IntPtr)18446744073709551551), unchecked((IntPtr)18446744073709551552)},
            {unchecked((IntPtr)18446744073709551552), unchecked((IntPtr)18446744073709551552)}
        };

    [Fact]
    public void TestAlign()
    {
        foreach (var test in s_tests)
        {
            var raw = test.Key;
            var aligned = Utilities.Align(raw, ALIGNMENT);

            Assert.Equal(aligned, Utilities.Align(raw, ALIGNMENT));
            Assert.InRange((ulong)aligned, (ulong)raw, (ulong)raw + ALIGNMENT - 1);
            Assert.Equal(0UL, (ulong)aligned % ALIGNMENT);
        }
    }
}