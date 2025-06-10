using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Services;

using Domain.Bitcoin.Interfaces;
using Domain.Protocol.Services;

public class DustService : IDustService
{
    private readonly IFeeService _feeService;

    public DustService(IFeeService feeService)
    {
        _feeService = feeService;
    }

    public ulong CalculateP2PkhDustLimit()
    {
        const uint outputSize = 34;
        const uint inputSize = 148;
        return CalculateDustLimit(outputSize, inputSize);
    }

    public ulong CalculateP2ShDustLimit()
    {
        const uint outputSize = 32;
        const uint inputSize = 148; // Lower bound
        return CalculateDustLimit(outputSize, inputSize);
    }

    public ulong CalculateP2WpkhDustLimit()
    {
        const uint outputSize = 31;
        const uint inputSize = 67; // Lower bound
        return CalculateDustLimit(outputSize, inputSize);
    }

    public ulong CalculateP2WshDustLimit()
    {
        const uint outputSize = 43;
        const uint inputSize = 67; // Lower bound
        return CalculateDustLimit(outputSize, inputSize);
    }

    public ulong CalculateUnknownSegwitVersionDustLimit()
    {
        const uint outputSize = 51;
        const uint inputSize = 67; // Lower bound
        return CalculateDustLimit(outputSize, inputSize);
    }

    private ulong CalculateDustLimit(uint outputSize, uint inputSize)
    {
        var totalSize = outputSize + inputSize;
        return (totalSize * _feeService.GetCachedFeeRatePerKw() / 1000);
    }

    public bool IsDust(ulong amount, Script scriptPubKey)
    {
        if (scriptPubKey.IsScriptType(ScriptType.P2PKH))
        {
            return amount < CalculateP2PkhDustLimit();
        }

        if (scriptPubKey.IsScriptType(ScriptType.P2SH))
        {
            return amount < CalculateP2ShDustLimit();
        }

        if (scriptPubKey.IsScriptType(ScriptType.P2WPKH))
        {
            return amount < CalculateP2WpkhDustLimit();
        }

        if (scriptPubKey.IsScriptType(ScriptType.P2WSH))
        {
            return amount < CalculateP2WshDustLimit();
        }

        if (scriptPubKey.ToBytes()[0] == (byte)OpcodeType.OP_RETURN)
        {
            return false; // OP_RETURN outputs are never dust
        }

        return amount < CalculateUnknownSegwitVersionDustLimit();
    }
}