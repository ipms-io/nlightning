using NBitcoin;
using NLightning.Bolts.BOLT3.Calculators;

namespace NLightning.Bolts.BOLT3.Services;

public class DustService
{
    private readonly FeeCalculator _feeCalculator;

    public DustService(FeeCalculator feeCalculator)
    {
        _feeCalculator = feeCalculator;
    }

    public ulong CalculateP2PkhDustLimit()
    {
        const uint OUTPUT_SIZE = 34;
        const uint INPUT_SIZE = 148;
        return CalculateDustLimit(OUTPUT_SIZE, INPUT_SIZE);
    }

    public ulong CalculateP2ShDustLimit()
    {
        const uint OUTPUT_SIZE = 32;
        const uint INPUT_SIZE = 148; // Lower bound
        return CalculateDustLimit(OUTPUT_SIZE, INPUT_SIZE);
    }

    public ulong CalculateP2WpkhDustLimit()
    {
        const uint OUTPUT_SIZE = 31;
        const uint INPUT_SIZE = 67; // Lower bound
        return CalculateDustLimit(OUTPUT_SIZE, INPUT_SIZE);
    }

    public ulong CalculateP2WshDustLimit()
    {
        const uint OUTPUT_SIZE = 43;
        const uint INPUT_SIZE = 67; // Lower bound
        return CalculateDustLimit(OUTPUT_SIZE, INPUT_SIZE);
    }

    public ulong CalculateUnknownSegwitVersionDustLimit()
    {
        const uint OUTPUT_SIZE = 51;
        const uint INPUT_SIZE = 67; // Lower bound
        return CalculateDustLimit(OUTPUT_SIZE, INPUT_SIZE);
    }

    private ulong CalculateDustLimit(uint outputSize, uint inputSize)
    {
        var totalSize = outputSize + inputSize;
        return (totalSize * _feeCalculator.GetCurrentEstimatedFeePerKw() / 1000);
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