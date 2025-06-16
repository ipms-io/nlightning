using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.Protocol.Tlv;
using NLightning.Infrastructure.Converters;

namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

public class RemoteAddressTlvConverter : ITlvConverter<RemoteAddressTlv>
{
    public BaseTlv ConvertToBase(RemoteAddressTlv tlv)
    {
        var tlvValue = new byte[tlv.Length];
        tlvValue[0] = tlv.AddressType;

        switch (tlv.AddressType)
        {
            // IPv4
            case 1:
                {
                    var ipAddressBytes = IPAddress.Parse(tlv.Address).GetAddressBytes();
                    if (ipAddressBytes.Length != 4)
                        throw new InvalidOperationException("Invalid IPv4 address");

                    ipAddressBytes.CopyTo(tlvValue, 1);
                    EndianBitConverter.GetBytesBigEndian(tlv.Port).CopyTo(tlvValue, 5);
                    break;
                }
            // IPv6
            case 2:
                {
                    var ipAddressBytes = IPAddress.Parse(tlv.Address).GetAddressBytes();
                    if (ipAddressBytes.Length != 16)
                        throw new InvalidOperationException("Invalid IPv6 address");

                    ipAddressBytes.CopyTo(tlvValue, 1);
                    EndianBitConverter.GetBytesBigEndian(tlv.Port).CopyTo(tlvValue, 17);
                    break;
                }
            // Tor v3
            case 4:
                {
                    var torV3Bytes = Convert.FromHexString(tlv.Address);
                    torV3Bytes.CopyTo(tlvValue, 1);
                    EndianBitConverter.GetBytesBigEndian(tlv.Port).CopyTo(tlvValue, 36);
                    break;
                }
            // Custom address type
            case 5:
                {
                    var customAddressBytes = Encoding.ASCII.GetBytes(tlv.Address);
                    customAddressBytes[1] = (byte)customAddressBytes.Length;
                    customAddressBytes.CopyTo(tlvValue, 2);
                    EndianBitConverter.GetBytesBigEndian(tlv.Port).CopyTo(tlvValue, customAddressBytes.Length + 2);
                    break;
                }
            default:
                throw new InvalidOperationException("Invalid address type");
        }

        return new BaseTlv(tlv.Type, tlvValue);
    }

    public RemoteAddressTlv ConvertFromBase(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.RemoteAddress)
            throw new InvalidCastException("Invalid TLV type");

        var addressType = baseTlv.Value[0];
        string address;
        ushort port;

        switch (addressType)
        {
            // IPv4
            case 1:
                if (baseTlv.Length != 7)
                    throw new InvalidCastException("Invalid length for IPv4 address");
                address = new IPAddress(baseTlv.Value[1..5]).ToString();
                port = EndianBitConverter.ToUInt16BigEndian(baseTlv.Value.AsSpan()[5..]);
                break;
            // IPv6
            case 2:
                if (baseTlv.Length != 19)
                    throw new InvalidCastException("Invalid length for IPv6 address");
                address = new IPAddress(baseTlv.Value[1..17]).ToString();
                port = EndianBitConverter.ToUInt16BigEndian(baseTlv.Value.AsSpan()[17..]);
                break;
            // Tor v3
            case 4:
                if (baseTlv.Length != 38)
                    throw new InvalidCastException("Invalid length for Tor v3 address");
                address = Convert.ToHexString(baseTlv.Value[1..37]);
                port = EndianBitConverter.ToUInt16BigEndian(baseTlv.Value.AsSpan()[37..]);
                break;
            // Custom address type
            case 5:
                if (baseTlv.Length < 3 || baseTlv.Length > 258)
                    throw new InvalidCastException("Invalid length for custom address");
                var customAddressLength = baseTlv.Value[1];
                if (customAddressLength + 3 != baseTlv.Length)
                    throw new InvalidCastException("Custom address length mismatch");
                address = Encoding.ASCII.GetString(baseTlv.Value[2..(2 + customAddressLength)]);
                port = EndianBitConverter.ToUInt16BigEndian(baseTlv.Value.AsSpan()[(2 + customAddressLength)..]);
                break;
            default:
                throw new InvalidOperationException("Invalid address type");
        }

        return new RemoteAddressTlv(addressType, address, port);
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as RemoteAddressTlv
                          ?? throw new InvalidCastException($"Error converting BaseTlv to {nameof(RemoteAddressTlv)}"));
    }
}