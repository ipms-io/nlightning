namespace NLightning.Bolts.Tests.BOLT8.Noise.Mock;

using static TestUtils;

internal sealed class ResponderValidKeys
{
    public byte[] LocalStaticPrivateKey { get; }
    public byte[] LocalStaticPublicKey { get; }
    public byte[] EphemeralPrivateKey { get; }
    public byte[] ActOneInput { get; }
    public byte[] ActTwoOutput { get; }
    public byte[] ActThreeInput { get; }
    public byte[] OutputSk { get; }
    public byte[] OutputRk { get; }

    public ResponderValidKeys()
    {
        LocalStaticPrivateKey = GetBytes("0x2121212121212121212121212121212121212121212121212121212121212121");
        LocalStaticPublicKey = GetBytes("0x028d7500dd4c12685d1f568b4c2b5048e8534b873319f3a8daa612b469132ec7f7");
        EphemeralPrivateKey = GetBytes("0x2222222222222222222222222222222222222222222222222222222222222222");
        ActOneInput = GetBytes("0x00036360e856310ce5d294e8be33fc807077dc56ac80d95d9cd4ddbd21325eff73f70df6086551151f58b8afe6c195782c6a");
        ActTwoOutput = GetBytes("0x0002466d7fcae563e5cb09a0d1870bb580344804617879a14949cf22285f1bae3f276e2470b93aac583c9ef6eafca3f730ae");
        ActThreeInput = GetBytes("0x00b9e3a702e93e3a9948c2ed6e5fd7590a6e1c3a0344cfc9d5b57357049aa22355361aa02e55a8fc28fef5bd6d71ad0c38228dc68b1c466263b47fdf31e560e139ba");
        OutputSk = GetBytes("0xbb9020b8965f4df047e07f955f3c4b88418984aadc5cdb35096b9ea8fa5c3442");
        OutputRk = GetBytes("0x969ab31b4d288cedf6218839b27a3e2140827047f2c0f01bf5c04435d43511a9");
    }
}