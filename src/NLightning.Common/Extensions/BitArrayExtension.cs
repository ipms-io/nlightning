using System.Collections;

namespace NLightning.Common.Extensions;

public static class BitArrayExtension
{
    public static int GetLastIndexOfOne(this BitArray bitArray)
    {
        for (var i = bitArray.Length - 1; i >= 0; i--)
        {
            if (bitArray[i])
            {
                return i;
            }
        }
        return -1; // Return -1 if no 1 is found
    }

    public static int GetFirstIndexOfOne(this BitArray bitArray)
    {
        for (var i = 0; i < bitArray.Length; i++)
        {
            if (bitArray[i])
            {
                return i;
            }
        }
        return -1; // Return -1 if no 1 is found
    }
}