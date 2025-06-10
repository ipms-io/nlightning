using NBitcoin;
using NLightning.Domain.Money;

namespace NLightning.Tests.Utils.Vectors;

public static class Bolt3AppendixBVectors
{
    public static readonly uint256 InputTxId = new("fd2105607605d2302994ffea703b09f66b6351816ee737a93e42a841ea20bbad");

    public static readonly Transaction InputTx = Transaction.Parse(
        "01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff03510101ffffffff0100f2052a010000001976a9143ca33c2e4446f4a305f23c80df8ad1afdcf652f988ac00000000",
        Network.Main);

    public const int InputIndex = 0;

    public static readonly Key InputSigningPrivKey =
        new(Convert.FromHexString("6bd078650fcee8444e4e09825227b801a1ca928debb750eb36e6d56124bb20e8"));

    public static readonly PubKey LocalPubKey =
        new("023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb");

    public static readonly PubKey RemotePubKey =
        new("030e9f7b623d2ccc7c9bd44d66d5ce21ce504c0acf6385a132cec6d3c39fa711c1");

    public static readonly LightningMoney FundingSatoshis = 10_000_000_000UL;

    public static readonly Script
        ChangeScript = Script.FromHex("00143ca33c2e4446f4a305f23c80df8ad1afdcf652f9"); // P2WPKH

    public static readonly LightningMoney ExpectedChangeSatoshis = 4_989_986_080_000UL;

    public static readonly uint256 ExpectedTxId =
        new("8984484a580b825b9972d7adb15050b3ab624ccd731946b3eeddb92f4e7ef6be");

    public static readonly Transaction ExpectedTx = Transaction.Parse(
        "0200000001adbb20ea41a8423ea937e76e8151636bf6093b70eaff942930d20576600521fd000000006b48304502210090587b6201e166ad6af0227d3036a9454223d49a1f11839c1a362184340ef0240220577f7cd5cca78719405cbf1de7414ac027f0239ef6e214c90fcaab0454d84b3b012103535b32d5eb0a6ed0982a0479bbadc9868d9836f6ba94dd5a63be16d875069184ffffffff028096980000000000220020c015c4a6be010e21657068fc2e6a9d02b27ebe4d490a25846f7237f104d1a3cd20256d29010000001600143ca33c2e4446f4a305f23c80df8ad1afdcf652f900000000",
        Network.Main);

    public static readonly WitScript InputWitScript =
        new(
            "5221023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb21030e9f7b623d2ccc7c9bd44d66d5ce21ce504c0acf6385a132cec6d3c39fa711c152ae");
}