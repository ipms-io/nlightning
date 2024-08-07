// using NBitcoin;
//
// namespace NLightning.Bolts.Tests.BOLT11.Types.TaggedFields;
//
// using Bolts.BOLT11.Enums;
// using Bolts.BOLT11.Types.TaggedFields;
// using Common.BitUtils;
// using NLightning.Bolts.BOLT11.Encoders;
//
// public class DescriptionHashTaggedFieldTests
// {
//     private readonly uint256 _descriptionHash = new("1863143c14c5166804bd19203356da136c985678cd4d27a1b8c6329604903262");
//     private readonly byte[] _descriptionHashData;
//     private readonly short _length;
//
//     public DescriptionHashTaggedFieldTests()
//     {
//         // Setup data encoding
//         var descriptionBytes = _descriptionHash.ToBytes();
//
//         // Convert from 8 bits to 5 bits and back
//         Bech32Encoder.ConvertBits(descriptionBytes, 8, 5, out var data);
//         _length = (short)data.Length;
//         Bech32Encoder.ConvertBits(data, 5, 8, out _descriptionHashData);
//     }
//
//     [Fact]
//     public void Given_Value_When_Constructed_Then_SetsPropertiesCorrectly()
//     {
//         // Act
//         var taggedField = new DescriptionHashTaggedField(_descriptionHash);
//
//         // Assert
//         Assert.Equal(TaggedFieldTypes.DescriptionHash, taggedField.Type);
//         Assert.Equal(_descriptionHashData, taggedField.Data);
//         Assert.Equal(_descriptionHash, taggedField.Value);
//         Assert.Equal(_descriptionHash, taggedField.GetValue());
//         Assert.True(taggedField.IsValid());
//     }
//
//     [Fact]
//     public void Given_BitReader_When_Constructed_Then_SetsPropertiesCorrectly()
//     {
//         // Arrange
//         using var bitReader = new BitReader(_descriptionHashData);
//
//         // Act
//         var taggedField = new DescriptionHashTaggedField(bitReader, _length);
//
//         // Assert
//         Assert.Equal(TaggedFieldTypes.DescriptionHash, taggedField.Type);
//         Assert.Equal(_descriptionHashData, taggedField.Data);
//         Assert.Equal(_descriptionHash, taggedField.Value);
//         Assert.Equal(_descriptionHash, taggedField.GetValue());
//         Assert.True(taggedField.IsValid());
//     }
//
//     [Fact]
//     public void Given_Value_When_IsValidCalled_Then_ReturnsCorrectly()
//     {
//         // Arrange
//         var validTaggedField = new DescriptionHashTaggedField(new uint256("1863143c14c5166804bd19203356da136c985678cd4d27a1b8c6329604903262"));
//         var invalidTaggedField = new DescriptionHashTaggedField(new uint256());
//
//         // Act & Assert
//         Assert.True(validTaggedField.IsValid());
//         Assert.False(invalidTaggedField.IsValid());
//     }
// }