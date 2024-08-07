// using System.Text;
//
// namespace NLightning.Bolts.Tests.BOLT11.Types.TaggedFields;
//
// using Bolts.BOLT11.Enums;
// using Bolts.BOLT11.Types.TaggedFields;
// using Common.BitUtils;
// using NLightning.Bolts.BOLT11.Encoders;
//
// public class DescriptionTaggedFieldTests
// {
//     private readonly string _description = "Test Description";
//     private readonly byte[] _descriptionData;
//     private readonly short _length;
//
//     public DescriptionTaggedFieldTests()
//     {
//         // Setup data encoding
//         var descriptionBytes = Encoding.UTF8.GetBytes(_description);
//
//         // Convert from 8 bits to 5 bits and back
//         Bech32Encoder.ConvertBits(descriptionBytes, 8, 5, out var data);
//         _length = (short)data.Length;
//         Bech32Encoder.ConvertBits(data, 5, 8, out _descriptionData);
//     }
//
//     [Fact]
//     public void Given_Value_When_Constructed_Then_SetsPropertiesCorrectly()
//     {
//         // Act
//         var taggedField = new DescriptionTaggedField(_description);
//
//         // Assert
//         Assert.Equal(TaggedFieldTypes.Description, taggedField.Type);
//         Assert.Equal(_descriptionData, taggedField.Data);
//         Assert.Equal(_description, taggedField.Value);
//         Assert.Equal(_description, taggedField.GetValue());
//         Assert.True(taggedField.IsValid());
//     }
//
//     [Fact]
//     public void Given_BitReader_When_Constructed_Then_SetsPropertiesCorrectly()
//     {
//         // Arrange
//         using var bitReader = new BitReader(_descriptionData);
//
//         // Act
//         var taggedField = new DescriptionTaggedField(bitReader, _length);
//
//         // Assert
//         Assert.Equal(TaggedFieldTypes.Description, taggedField.Type);
//         Assert.Equal(_descriptionData[..^1], taggedField.Data);
//         Assert.Equal(_description, taggedField.Value);
//         Assert.Equal(_description, taggedField.GetValue());
//         Assert.True(taggedField.IsValid());
//     }
//
//     [Fact]
//     public void Given_Value_When_IsNotValidCalled_Then_ExceptionIsThrown()
//     {
//         // Act & Assert
//         Assert.ThrowsAny<ArgumentException>(() => new DescriptionTaggedField(string.Empty));
//     }
// }