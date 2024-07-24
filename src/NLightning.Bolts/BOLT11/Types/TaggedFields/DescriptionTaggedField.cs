using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using BOLT11.Enums;

public sealed class DescriptionTaggedField : BaseTaggedField<string>
{
    [SetsRequiredMembers]
    public DescriptionTaggedField(BitReader buffer, int length) : base(TaggedFieldTypes.Description, length)
    {
        buffer.ReadBits(Data, length * 5);
        if (length * 5 % 8 > 0)
        {
            Data[^1] = 0;
        }
        Value = Decode(Data);
    }

    protected override string Decode(byte[] data)
    {
        return Encoding.UTF8.GetString(data);
    }

    public override bool IsValid()
    {
        return Value != null;
    }
}