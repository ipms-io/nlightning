namespace NLightning.Bolts.BOLT3.Constants;

public static class WeightConstants
{
    // ReSharper disable InconsistentNaming
    //                                                     | Amount    | Script Length | Script     |             
    public const int P2PKH_OUTPUT_WEIGHT = 34 * 4; // | 8         | 1             | 25         |
    public const int P2SH_OUTPUT_WEIGHT = 33 * 4; // | 8         | 1             | 23         |
    public const int P2WPKH_OUTPUT_WEIGHT = 31 * 4; // | 8         | 1             | 22         |
    public const int P2WSH_OUTPUT_WEIGHT = 43 * 4; // | 8         | 1             | 34         |
    public const int P2UNKOWN_S_OUTPUT_WEIGHT = 51 * 4; // | 8         | 1             | 42         |

    public const int P2PKH_INTPUT_WEIGHT = 148; // At Least
    public const int P2SH_INTPUT_WEIGHT = 148; // At Least
    public const int P2WPKH_INTPUT_WEIGHT = 41; // At Least
    public const int P2WSH_INTPUT_WEIGHT = P2WPKH_INTPUT_WEIGHT;
    public const int P2UNKOWN_S_INTPUT_WEIGHT = P2WPKH_INTPUT_WEIGHT;
    // ReSharper enable InconsistentNaming

    public const int WITNESS_HEADER = 2; // flag, marker
    public const int MULTISIG_WITNESS_WEIGHT = 222; // 1 byte for each signature

    public const int HTLC_OUTPUT_WEIGHT = P2WSH_OUTPUT_WEIGHT;
    public const int ANCHOR_OUTPUT_WEIGHT = P2WSH_OUTPUT_WEIGHT;

    public const int TRANSACTION_BASE_WEIGHT = 10 * 4; // version, input count, output count, locktime
}