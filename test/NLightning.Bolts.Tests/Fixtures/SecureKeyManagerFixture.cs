namespace NLightning.Bolts.Tests.Fixtures;

using Common.Managers;

// ReSharper disable once ClassNeverInstantiated.Global
public class SecureKeyManagerFixture
{
    public const string PRIVATE_KEY_HEX_STRING = "e126f68f7eafcc8b74f54d269fe206be715000f94dac067d1c04a8ca3b2db734";

    public readonly byte[] PRIVATE_KEY_BYTES;

    public SecureKeyManagerFixture()
    {
        PRIVATE_KEY_BYTES = NBitcoin.DataEncoders.Encoders.Hex.DecodeData(PRIVATE_KEY_HEX_STRING);
        SecureKeyManager.Initialize(PRIVATE_KEY_BYTES);
    }
}