using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Common.TLVs;
using Common.Types;
using Exceptions;
using Utils;

public class UpdateAddHtlcMessageTests
{
    #region Deserialize
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsUpdateAddHtlcMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedId = 0UL;
        var expectedAmountMsat = 1UL;
        var expectedPaymentHash = "567cbdadb00b825448b2e414487d73a97f657f0634166d3ab3f3a2cc1042eda5".ToByteArray();
        var expectedCltvExpiry = 3u;
        var stream = new MemoryStream("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA500000003".ToByteArray());

        // Act
        var message = await UpdateAddHtlcMessage.DeserializeAsync(stream);

        // Assert
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedId, message.Payload.Id);
        Assert.Equal(expectedAmountMsat, message.Payload.AmountMsats);
        Assert.Equal(expectedPaymentHash, message.Payload.PaymentHash);
        Assert.Equal(expectedCltvExpiry, message.Payload.CltvExpiry);
        Assert.Null(message.Extension);
    }

    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsUpdateAddHtlcMessageWithRoutingPacket()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedId = 0UL;
        var expectedAmountMsat = 1UL;
        var expectedPaymentHash = "567cbdadb00b825448b2e414487d73a97f657f0634166d3ab3f3a2cc1042eda5".ToByteArray();
        var expectedCltvExpiry = 3u;
        var expectedOnionRoutingPacket = "567cbdadb00b825448b2e414487d73a97f657f0634166d3ab3f3a2cc1042eda518885f08987b365412fdffa239917499b5b45557c6312852b36c62b5bd0c3f6837bcd5f6757b564cc44090ee1c156621ef432e9f0ffacb77dfca219d514312ae02f6c0d865cea05074183c6c6300c8ffbb3fdacc9e01847d32567e9d189ab01aa4a66f4f12ba54202f10d11604b00cda31c259d24a14b8816940d3b6ce20b955687ee834f07e35cbffadbe725588f1d64985ff329a1860afb0cbe1c81b4028209a6fccf0c44f18a0d1e7d2e10b800aee3ddfcae4395cb363a840f8958ec860d51903a89d60ffaba46256d68a15920544ca989469e18e6dc252c2edbd153292af3dbe51c8d9064360588dc7316a8e682d56ea99f5ef6da6e8f566e715a3320396b556cc0bfad6ad22f25635a893cd493734c7667834005ff5ad2a437a0eada662823382cec1164661e691845a81ea063dc6dbd84c30bcfb8de272a14bc46601c1c0d4216d96685f6272d60313b3034aec74b11ad0639b885f22de2a476a989c7198f1aa556f50abd3d8d60b9da0ae2a31bd7e744723b33a44a68b19b60bad95c90d79db77fadf8852afff7cb44882ba7b06741fd68bdd25f27c120c9b42ffa544cca0a4350cb1fe43030a8bbd06b584114c4e8101386c0603e90f657c8e105da42bf8bb3c9dbae55e008ff25ca141f630f6b230e863c61a4d9affacfecd2580abd42087d26de8a6371af07f6f532d482a7026e4cc1f2295be284e5018655fcb7ae272b33a0f209f80b1a77eb8102aeb98eb85b77047e8a6ddfcd48b31175f4177cca1d3b0942fea2fc6d8f71492f3a260abb7eebedac4cef600b6d65a156df8e194b86461b752cd6f289feed9ab53d8a977bec9dab73774c7bb7c60e02f728a61598df8fedbc851cdd4e6b97641fb3d11a15320957d9e2fc2732c210d463b880fb1a5b5ca18e6ad456b8acd1d9dfe43824c0b12a925efea55bf74380ca6a4dfbb267a5c61299eef9663ffaf40d2a8c078be6d95bf5cd3d8fddf3ac76a76b55ac6d3ef964ee5b7e1434cadd1a66c33b4b75babe907fcc3fe97ead7d6d7faa05b9184c2e54aa8f8366e6fdc6d9d0fca8dafb5adf3f31505b15c89a12063cd7492f392ca575404ff9c93c7935a9f1c5d28b88e63d0df7db36dfb2c498f8ff665b2bc01718eebfa47adbf34aac7c34834824ab3b101aca7a09e3210c4aca6ab0b7d214cf7e69d992ec0231ef1d2afc19b09f035baca8f04fcdc6adaab392732ac1c223ef3af0d6020e3d41a1c7766590ee88e04b8161c5ae21f7e94f7dc3a2085d4fb54d2ecd1772db2f6cac66354d7e522e99574b0e952e3f4ee06b4d5047f2d2149ab03dde085daca6771044a15c6d956096d9c2dd5cda17e230195c676cce9ece97a83957f2520d844350d1c72766e80b421e6d9fbef30f8f8223ac5b1e7b9e49f0083b90fd42af4b0cd60633de954a04101bba9a87f3b7bff60b3d5806828fde024437b7b7c97db35b93f8cfc98b19a15e92a65ec7c918c913c1e02593b080d8700ec65948d954b49d1d9099e9bcf50ea201e8d09b1f92245bcabcc55226ec8599e4530fe8d3c645c88cdd1dd483086341c89d36b94db6964a6acf029dd200565eb7eed8570168c7e3e8fe2e49dd1e706d38e3ecd3693d41b279a1ed6e090be23bf359db63a5b2acef5432b482af1f5ad2c38e288a0ae5fa49093789509e7aa167c53c6e6faa60e009edf15b8a0e5263822cbcc32177ee28d6320ead4c60d30b67e75fae676fc9f38a784db7612104a20aaba108457dbb09e403f120b1019d23b7c3b095f50007c388f74eec12e7cb45cc45769939a88026f2f3bdd29f47dd480b54aa6606b42fdc191d69d107e0c94f39f5f760534b448b006d7e07faf92d70d2cdbfafe799a76e4f19a73f00bb0d06b50cf09955659".ToByteArray();
        var stream = new MemoryStream("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA500000003567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA518885F08987B365412FDFFA239917499B5B45557C6312852B36C62B5BD0C3F6837BCD5F6757B564CC44090EE1C156621EF432E9F0FFACB77DFCA219D514312AE02F6C0D865CEA05074183C6C6300C8FFBB3FDACC9E01847D32567E9D189AB01AA4A66F4F12BA54202F10D11604B00CDA31C259D24A14B8816940D3B6CE20B955687EE834F07E35CBFFADBE725588F1D64985FF329A1860AFB0CBE1C81B4028209A6FCCF0C44F18A0D1E7D2E10B800AEE3DDFCAE4395CB363A840F8958EC860D51903A89D60FFABA46256D68A15920544CA989469E18E6DC252C2EDBD153292AF3DBE51C8D9064360588DC7316A8E682D56EA99F5EF6DA6E8F566E715A3320396B556CC0BFAD6AD22F25635A893CD493734C7667834005FF5AD2A437A0EADA662823382CEC1164661E691845A81EA063DC6DBD84C30BCFB8DE272A14BC46601C1C0D4216D96685F6272D60313B3034AEC74B11AD0639B885F22DE2A476A989C7198F1AA556F50ABD3D8D60B9DA0AE2A31BD7E744723B33A44A68B19B60BAD95C90D79DB77FADF8852AFFF7CB44882BA7B06741FD68BDD25F27C120C9B42FFA544CCA0A4350CB1FE43030A8BBD06B584114C4E8101386C0603E90F657C8E105DA42BF8BB3C9DBAE55E008FF25CA141F630F6B230E863C61A4D9AFFACFECD2580ABD42087D26DE8A6371AF07F6F532D482A7026E4CC1F2295BE284E5018655FCB7AE272B33A0F209F80B1A77EB8102AEB98EB85B77047E8A6DDFCD48B31175F4177CCA1D3B0942FEA2FC6D8F71492F3A260ABB7EEBEDAC4CEF600B6D65A156DF8E194B86461B752CD6F289FEED9AB53D8A977BEC9DAB73774C7BB7C60E02F728A61598DF8FEDBC851CDD4E6B97641FB3D11A15320957D9E2FC2732C210D463B880FB1A5B5CA18E6AD456B8ACD1D9DFE43824C0B12A925EFEA55BF74380CA6A4DFBB267A5C61299EEF9663FFAF40D2A8C078BE6D95BF5CD3D8FDDF3AC76A76B55AC6D3EF964EE5B7E1434CADD1A66C33B4B75BABE907FCC3FE97EAD7D6D7FAA05B9184C2E54AA8F8366E6FDC6D9D0FCA8DAFB5ADF3F31505B15C89A12063CD7492F392CA575404FF9C93C7935A9F1C5D28B88E63D0DF7DB36DFB2C498F8FF665B2BC01718EEBFA47ADBF34AAC7C34834824AB3B101ACA7A09E3210C4ACA6AB0B7D214CF7E69D992EC0231EF1D2AFC19B09F035BACA8F04FCDC6ADAAB392732AC1C223EF3AF0D6020E3D41A1C7766590EE88E04B8161C5AE21F7E94F7DC3A2085D4FB54D2ECD1772DB2F6CAC66354D7E522E99574B0E952E3F4EE06B4D5047F2D2149AB03DDE085DACA6771044A15C6D956096D9C2DD5CDA17E230195C676CCE9ECE97A83957F2520D844350D1C72766E80B421E6D9FBEF30F8F8223AC5B1E7B9E49F0083B90FD42AF4B0CD60633DE954A04101BBA9A87F3B7BFF60B3D5806828FDE024437B7B7C97DB35B93F8CFC98B19A15E92A65EC7C918C913C1E02593B080D8700EC65948D954B49D1D9099E9BCF50EA201E8D09B1F92245BCABCC55226EC8599E4530FE8D3C645C88CDD1DD483086341C89D36B94DB6964A6ACF029DD200565EB7EED8570168C7E3E8FE2E49DD1E706D38E3ECD3693D41B279A1ED6E090BE23BF359DB63A5B2ACEF5432B482AF1F5AD2C38E288A0AE5FA49093789509E7AA167C53C6E6FAA60E009EDF15B8A0E5263822CBCC32177EE28D6320EAD4C60D30B67E75FAE676FC9F38A784DB7612104A20AABA108457DBB09E403F120B1019D23B7C3B095F50007C388F74EEC12E7CB45CC45769939A88026F2F3BDD29F47DD480B54AA6606B42FDC191D69D107E0C94F39F5F760534B448B006D7E07FAF92D70D2CDBFAFE799A76E4F19A73F00BB0D06B50CF09955659".ToByteArray());

        // Act
        var message = await UpdateAddHtlcMessage.DeserializeAsync(stream);

        // Assert
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedId, message.Payload.Id);
        Assert.Equal(expectedAmountMsat, message.Payload.AmountMsats);
        Assert.Equal(expectedPaymentHash, message.Payload.PaymentHash);
        Assert.Equal(expectedCltvExpiry, message.Payload.CltvExpiry);
        Assert.Equal(expectedOnionRoutingPacket, message.Payload.OnionRoutingPacket!.Value.ToArray());
        Assert.Null(message.Extension);
    }

    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxAckRbfMessageWithExtensions()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedId = 0UL;
        var expectedAmountMsat = 1UL;
        var expectedPaymentHash = "567cbdadb00b825448b2e414487d73a97f657f0634166d3ab3f3a2cc1042eda5".ToByteArray();
        var expectedCltvExpiry = 3u;
        var expectedPathKey = new PubKey("02c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75".ToByteArray());
        var stream = new MemoryStream("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA500000003002102C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A75".ToByteArray());

        // Act
        var message = await UpdateAddHtlcMessage.DeserializeAsync(stream);

        // Assert
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedId, message.Payload.Id);
        Assert.Equal(expectedAmountMsat, message.Payload.AmountMsats);
        Assert.Equal(expectedPaymentHash, message.Payload.PaymentHash);
        Assert.Equal(expectedCltvExpiry, message.Payload.CltvExpiry);
        Assert.NotNull(message.Extension);
        Assert.NotNull(message.BlindedPathTlv);
        Assert.Equal(expectedPathKey, message.BlindedPathTlv.PathKey);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream("0080000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA5000000030021".ToByteArray());

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => UpdateAddHtlcMessage.DeserializeAsync(invalidStream));
    }
    #endregion

    #region Serialize
    [Fact]
    public async Task Given_ValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var id = 0UL;
        var amountMsat = 1UL;
        var paymentHash = "567cbdadb00b825448b2e414487d73a97f657f0634166d3ab3f3a2cc1042eda5".ToByteArray();
        var cltvExpiry = 3u;
        var message = new UpdateAddHtlcMessage(new UpdateAddHtlcPayload(channelId, id, amountMsat, paymentHash, cltvExpiry));
        var stream = new MemoryStream();
        var expectedBytes = "0080000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA500000003".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }

    [Fact]
    public async Task Given_ValidPayloadWithOnionPacket_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var id = 0UL;
        var amountMsat = 1UL;
        var paymentHash = "567cbdadb00b825448b2e414487d73a97f657f0634166d3ab3f3a2cc1042eda5".ToByteArray();
        var cltvExpiry = 3u;
        var onionRoutingPacket = "567cbdadb00b825448b2e414487d73a97f657f0634166d3ab3f3a2cc1042eda518885f08987b365412fdffa239917499b5b45557c6312852b36c62b5bd0c3f6837bcd5f6757b564cc44090ee1c156621ef432e9f0ffacb77dfca219d514312ae02f6c0d865cea05074183c6c6300c8ffbb3fdacc9e01847d32567e9d189ab01aa4a66f4f12ba54202f10d11604b00cda31c259d24a14b8816940d3b6ce20b955687ee834f07e35cbffadbe725588f1d64985ff329a1860afb0cbe1c81b4028209a6fccf0c44f18a0d1e7d2e10b800aee3ddfcae4395cb363a840f8958ec860d51903a89d60ffaba46256d68a15920544ca989469e18e6dc252c2edbd153292af3dbe51c8d9064360588dc7316a8e682d56ea99f5ef6da6e8f566e715a3320396b556cc0bfad6ad22f25635a893cd493734c7667834005ff5ad2a437a0eada662823382cec1164661e691845a81ea063dc6dbd84c30bcfb8de272a14bc46601c1c0d4216d96685f6272d60313b3034aec74b11ad0639b885f22de2a476a989c7198f1aa556f50abd3d8d60b9da0ae2a31bd7e744723b33a44a68b19b60bad95c90d79db77fadf8852afff7cb44882ba7b06741fd68bdd25f27c120c9b42ffa544cca0a4350cb1fe43030a8bbd06b584114c4e8101386c0603e90f657c8e105da42bf8bb3c9dbae55e008ff25ca141f630f6b230e863c61a4d9affacfecd2580abd42087d26de8a6371af07f6f532d482a7026e4cc1f2295be284e5018655fcb7ae272b33a0f209f80b1a77eb8102aeb98eb85b77047e8a6ddfcd48b31175f4177cca1d3b0942fea2fc6d8f71492f3a260abb7eebedac4cef600b6d65a156df8e194b86461b752cd6f289feed9ab53d8a977bec9dab73774c7bb7c60e02f728a61598df8fedbc851cdd4e6b97641fb3d11a15320957d9e2fc2732c210d463b880fb1a5b5ca18e6ad456b8acd1d9dfe43824c0b12a925efea55bf74380ca6a4dfbb267a5c61299eef9663ffaf40d2a8c078be6d95bf5cd3d8fddf3ac76a76b55ac6d3ef964ee5b7e1434cadd1a66c33b4b75babe907fcc3fe97ead7d6d7faa05b9184c2e54aa8f8366e6fdc6d9d0fca8dafb5adf3f31505b15c89a12063cd7492f392ca575404ff9c93c7935a9f1c5d28b88e63d0df7db36dfb2c498f8ff665b2bc01718eebfa47adbf34aac7c34834824ab3b101aca7a09e3210c4aca6ab0b7d214cf7e69d992ec0231ef1d2afc19b09f035baca8f04fcdc6adaab392732ac1c223ef3af0d6020e3d41a1c7766590ee88e04b8161c5ae21f7e94f7dc3a2085d4fb54d2ecd1772db2f6cac66354d7e522e99574b0e952e3f4ee06b4d5047f2d2149ab03dde085daca6771044a15c6d956096d9c2dd5cda17e230195c676cce9ece97a83957f2520d844350d1c72766e80b421e6d9fbef30f8f8223ac5b1e7b9e49f0083b90fd42af4b0cd60633de954a04101bba9a87f3b7bff60b3d5806828fde024437b7b7c97db35b93f8cfc98b19a15e92a65ec7c918c913c1e02593b080d8700ec65948d954b49d1d9099e9bcf50ea201e8d09b1f92245bcabcc55226ec8599e4530fe8d3c645c88cdd1dd483086341c89d36b94db6964a6acf029dd200565eb7eed8570168c7e3e8fe2e49dd1e706d38e3ecd3693d41b279a1ed6e090be23bf359db63a5b2acef5432b482af1f5ad2c38e288a0ae5fa49093789509e7aa167c53c6e6faa60e009edf15b8a0e5263822cbcc32177ee28d6320ead4c60d30b67e75fae676fc9f38a784db7612104a20aaba108457dbb09e403f120b1019d23b7c3b095f50007c388f74eec12e7cb45cc45769939a88026f2f3bdd29f47dd480b54aa6606b42fdc191d69d107e0c94f39f5f760534b448b006d7e07faf92d70d2cdbfafe799a76e4f19a73f00bb0d06b50cf09955659".ToByteArray();
        var message = new UpdateAddHtlcMessage(new UpdateAddHtlcPayload(channelId, id, amountMsat, paymentHash, cltvExpiry, onionRoutingPacket));
        var stream = new MemoryStream();
        var expectedBytes = "0080000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA500000003567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA518885F08987B365412FDFFA239917499B5B45557C6312852B36C62B5BD0C3F6837BCD5F6757B564CC44090EE1C156621EF432E9F0FFACB77DFCA219D514312AE02F6C0D865CEA05074183C6C6300C8FFBB3FDACC9E01847D32567E9D189AB01AA4A66F4F12BA54202F10D11604B00CDA31C259D24A14B8816940D3B6CE20B955687EE834F07E35CBFFADBE725588F1D64985FF329A1860AFB0CBE1C81B4028209A6FCCF0C44F18A0D1E7D2E10B800AEE3DDFCAE4395CB363A840F8958EC860D51903A89D60FFABA46256D68A15920544CA989469E18E6DC252C2EDBD153292AF3DBE51C8D9064360588DC7316A8E682D56EA99F5EF6DA6E8F566E715A3320396B556CC0BFAD6AD22F25635A893CD493734C7667834005FF5AD2A437A0EADA662823382CEC1164661E691845A81EA063DC6DBD84C30BCFB8DE272A14BC46601C1C0D4216D96685F6272D60313B3034AEC74B11AD0639B885F22DE2A476A989C7198F1AA556F50ABD3D8D60B9DA0AE2A31BD7E744723B33A44A68B19B60BAD95C90D79DB77FADF8852AFFF7CB44882BA7B06741FD68BDD25F27C120C9B42FFA544CCA0A4350CB1FE43030A8BBD06B584114C4E8101386C0603E90F657C8E105DA42BF8BB3C9DBAE55E008FF25CA141F630F6B230E863C61A4D9AFFACFECD2580ABD42087D26DE8A6371AF07F6F532D482A7026E4CC1F2295BE284E5018655FCB7AE272B33A0F209F80B1A77EB8102AEB98EB85B77047E8A6DDFCD48B31175F4177CCA1D3B0942FEA2FC6D8F71492F3A260ABB7EEBEDAC4CEF600B6D65A156DF8E194B86461B752CD6F289FEED9AB53D8A977BEC9DAB73774C7BB7C60E02F728A61598DF8FEDBC851CDD4E6B97641FB3D11A15320957D9E2FC2732C210D463B880FB1A5B5CA18E6AD456B8ACD1D9DFE43824C0B12A925EFEA55BF74380CA6A4DFBB267A5C61299EEF9663FFAF40D2A8C078BE6D95BF5CD3D8FDDF3AC76A76B55AC6D3EF964EE5B7E1434CADD1A66C33B4B75BABE907FCC3FE97EAD7D6D7FAA05B9184C2E54AA8F8366E6FDC6D9D0FCA8DAFB5ADF3F31505B15C89A12063CD7492F392CA575404FF9C93C7935A9F1C5D28B88E63D0DF7DB36DFB2C498F8FF665B2BC01718EEBFA47ADBF34AAC7C34834824AB3B101ACA7A09E3210C4ACA6AB0B7D214CF7E69D992EC0231EF1D2AFC19B09F035BACA8F04FCDC6ADAAB392732AC1C223EF3AF0D6020E3D41A1C7766590EE88E04B8161C5AE21F7E94F7DC3A2085D4FB54D2ECD1772DB2F6CAC66354D7E522E99574B0E952E3F4EE06B4D5047F2D2149AB03DDE085DACA6771044A15C6D956096D9C2DD5CDA17E230195C676CCE9ECE97A83957F2520D844350D1C72766E80B421E6D9FBEF30F8F8223AC5B1E7B9E49F0083B90FD42AF4B0CD60633DE954A04101BBA9A87F3B7BFF60B3D5806828FDE024437B7B7C97DB35B93F8CFC98B19A15E92A65EC7C918C913C1E02593B080D8700EC65948D954B49D1D9099E9BCF50EA201E8D09B1F92245BCABCC55226EC8599E4530FE8D3C645C88CDD1DD483086341C89D36B94DB6964A6ACF029DD200565EB7EED8570168C7E3E8FE2E49DD1E706D38E3ECD3693D41B279A1ED6E090BE23BF359DB63A5B2ACEF5432B482AF1F5AD2C38E288A0AE5FA49093789509E7AA167C53C6E6FAA60E009EDF15B8A0E5263822CBCC32177EE28D6320EAD4C60D30B67E75FAE676FC9F38A784DB7612104A20AABA108457DBB09E403F120B1019D23B7C3B095F50007C388F74EEC12E7CB45CC45769939A88026F2F3BDD29F47DD480B54AA6606B42FDC191D69D107E0C94F39F5F760534B448B006D7E07FAF92D70D2CDBFAFE799A76E4F19A73F00BB0D06B50CF09955659".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }

    [Fact]
    public async Task Given_ValidPayloadAndExtensions_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var id = 0UL;
        var amountMsat = 1UL;
        var paymentHash = "567cbdadb00b825448b2e414487d73a97f657f0634166d3ab3f3a2cc1042eda5".ToByteArray();
        var cltvExpiry = 3u;
        var pathKey = new PubKey("02c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75".ToByteArray());
        var blindedPathTlv = new BlindedPathTlv(pathKey);
        var message = new UpdateAddHtlcMessage(new UpdateAddHtlcPayload(channelId, id, amountMsat, paymentHash, cltvExpiry), blindedPathTlv);
        var stream = new MemoryStream();
        var expectedBytes = "0080000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA500000003002102C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A75".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
    #endregion
}