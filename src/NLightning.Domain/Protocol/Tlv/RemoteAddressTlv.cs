using System.Globalization;
using NLightning.Domain.Protocol.Constants;

namespace NLightning.Domain.Protocol.Tlv;

public class RemoteAddressTlv : BaseTlv
{
    public byte AddressType { get; }
    public string Address { get; }
    public ushort Port { get; }

    public RemoteAddressTlv(byte addressType, string address, ushort port) : base(TlvConstants.RemoteAddress)
    {
        if (addressType is < 1 or > 5)
            throw new ArgumentException("Invalid address type", nameof(addressType));

        if (addressType == 5)
            address = new IdnMapping().GetAscii(address);

        AddressType = addressType;
        Address = address;
        Port = port;

        Length = addressType switch
        {
            1 => 7,
            2 => 19,
            3 => throw new ArgumentException("Tor v2 addresses are deprecated", nameof(addressType)),
            4 => 38,
            5 when address.Length > 255 => throw new ArgumentException("Maximum lenght for address is 255",
                                                                       nameof(address)),
            5 => 3 + address.Length,
            _ => throw new ArgumentException("Invalid address type", nameof(addressType))
        };

        Value = new byte[Length];
    }
}