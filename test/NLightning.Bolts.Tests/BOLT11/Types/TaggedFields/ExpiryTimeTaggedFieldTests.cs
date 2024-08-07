// namespace NLightning.Bolts.Tests.BOLT11.Types.TaggedFields;
//
// using Bolts.BOLT11.Enums;
// using Bolts.BOLT11.Types.TaggedFields;
// using Common.BitUtils;
// using NLightning.Bolts.BOLT11.Encoders;
//
// public class ExpiryTimeTaggedFieldTests
// {
//     private readonly int _expiryTime = 60;
//     private readonly byte[] _expiryTimeData;
//     private readonly short _length;
//
//     public ExpiryTimeTaggedFieldTests()
//     {
//         // Setup data encoding
//         var expiryTimeBytes = EndianBitConverter.GetBytesBE(_expiryTime, true);
//
//         // Convert from 8 bits to 5 bits and back
//         Bech32Encoder.ConvertBits(expiryTimeBytes, 8, 5, out var data);
//         _length = (short)data.Length;
//         Bech32Encoder.ConvertBits(data, 5, 8, out _expiryTimeData);
//     }
//
//     [Fact]
//     public void Given_Value_When_Constructed_Then_SetsPropertiesCorrectly()
//     {
//         // Act
//         var taggedField = new ExpiryTimeTaggedField(_expiryTime);
//
//         // Assert
//         Assert.Equal(TaggedFieldTypes.ExpiryTime, taggedField.Type);
//         Assert.Equal(_expiryTimeData, taggedField.Data);
//         Assert.Equal(_expiryTime, taggedField.Value);
//         Assert.Equal(_expiryTime, taggedField.GetValue());
//         Assert.True(taggedField.IsValid());
//     }
//
//     [Fact]
//     public void Given_BitReader_When_Constructed_Then_SetsPropertiesCorrectly()
//     {
//         // Arrange
//         using var bitReader = new BitReader(_expiryTimeData);
//
//         // Act
//         var taggedField = new ExpiryTimeTaggedField(bitReader, _length);
//
//         // Assert
//         Assert.Equal(TaggedFieldTypes.ExpiryTime, taggedField.Type);
//         Assert.Equal(_expiryTimeData[..^1], taggedField.Data);
//         Assert.Equal(_expiryTime, taggedField.Value);
//         Assert.Equal(_expiryTime, taggedField.GetValue());
//         Assert.True(taggedField.IsValid());
//     }
//
//     [Fact]
//     public void Given_Value_When_IsValidCalled_Then_ReturnsCorrectly()
//     {
//         // Arrange
//         var validTaggedField = new ExpiryTimeTaggedField(3600);
//         var invalidTaggedField = new ExpiryTimeTaggedField(-1);
//
//         // Act & Assert
//         Assert.True(validTaggedField.IsValid());
//         Assert.False(invalidTaggedField.IsValid());
//     }
// }