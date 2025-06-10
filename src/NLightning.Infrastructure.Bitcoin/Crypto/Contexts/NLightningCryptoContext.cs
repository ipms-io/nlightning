using NBitcoin;
using NBitcoin.Secp256k1;

namespace NLightning.Infrastructure.Bitcoin.Crypto.Contexts;

internal static class NLightningCryptoContext
{
    private static readonly Lazy<Context> s_instance = new(CreateInstance, true);

    public static Context Instance => s_instance.Value;

    private static Context CreateInstance()
    {
        var gen = new ECMultGenContext();
        gen.Blind(RandomUtils.GetBytes(32));
        return new Context(new ECMultContext(), gen);
    }
}