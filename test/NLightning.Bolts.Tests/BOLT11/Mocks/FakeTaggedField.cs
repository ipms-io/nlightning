// using System.Diagnostics.CodeAnalysis;
//
// namespace NLightning.Bolts.Tests.BOLT11.Mocks;
//
// using Bolts.BOLT11.Enums;
// using Bolts.BOLT11.Types.TaggedFields;
// using Common.BitUtils;
//
// public class FakeTaggedField : BaseTaggedField<string>
// {
//     [SetsRequiredMembers]
//     public FakeTaggedField(TaggedFieldTypes type, string value) : base(type, value) { }
//
//     [SetsRequiredMembers]
//     public FakeTaggedField(TaggedFieldTypes type, BitReader bitReader, short length) : base(type, bitReader, length) { }
//
//     [SetsRequiredMembers]
//     public FakeTaggedField(TaggedFieldTypes type, short length) : base(type, length) { }
//
//     protected override string Decode(byte[] data)
//     {
//         return System.Text.Encoding.UTF8.GetString(data);
//     }
//
//     protected override byte[] Encode(string value)
//     {
//         return System.Text.Encoding.UTF8.GetBytes(value);
//     }
//
//     public override bool IsValid()
//     {
//         return !string.IsNullOrEmpty(Value);
//     }
//
//     // Public wrapper methods for testing
//     public byte[] PublicEncode(string value) => Encode(value);
//     public string PublicDecode(byte[] data) => Decode(data);
// }