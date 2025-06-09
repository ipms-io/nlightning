#if CRYPTO_NATIVE
using System.Buffers;
using System.Buffers.Binary;
using NLightning.Infrastructure.Crypto.Providers.Native.Constants;

namespace NLightning.Infrastructure.Crypto.Providers.Native.Ciphers;

using Converters;

public static class HChaCha20
{
    private static readonly uint[] s_hChacha20Constant = [0x61707865, 0x3320646E, 0x79622D32, 0x6B206574];

    public static void CreateInitialState(ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<uint> state)
    {
        if (state.Length != XChaCha20Constants.StateSize)
            throw new ArgumentException("State must be 16 bytes long", nameof(state));

        // set HChaCha20 constant
        s_hChacha20Constant.CopyTo(state);

        // set key
        Span<uint> keyState = stackalloc uint[8];
        ToUint32LittleEndian(key, keyState);
        keyState.CopyTo(state[4..]);

        // set nonce
        Span<uint> nonceState = stackalloc uint[4];
        ToUint32LittleEndian(nonce, nonceState);
        nonceState.CopyTo(state[^4..]);
    }

    public static void PerformRounds(Span<uint> state)
    {
        for (var i = 0; i < 10; i++)
        {
            ChaCha20.QuarterRound(ref state[0], ref state[4], ref state[8], ref state[12]);
            ChaCha20.QuarterRound(ref state[1], ref state[5], ref state[9], ref state[13]);
            ChaCha20.QuarterRound(ref state[2], ref state[6], ref state[10], ref state[14]);
            ChaCha20.QuarterRound(ref state[3], ref state[7], ref state[11], ref state[15]);
            ChaCha20.QuarterRound(ref state[0], ref state[5], ref state[10], ref state[15]);
            ChaCha20.QuarterRound(ref state[1], ref state[6], ref state[11], ref state[12]);
            ChaCha20.QuarterRound(ref state[2], ref state[7], ref state[8], ref state[13]);
            ChaCha20.QuarterRound(ref state[3], ref state[4], ref state[9], ref state[14]);
        }
    }

    public static void CreateSubkey(ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> subkey)
    {
        Span<uint> state = stackalloc uint[XChaCha20Constants.StateSize];
        CreateInitialState(key, nonce, state);
        PerformRounds(state);

        FromUint32LittleEndian([state[0], state[1], state[2], state[3], state[12], state[13], state[14], state[15]],
                               subkey);
    }

    private static void ToUint32LittleEndian(ReadOnlySpan<byte> buffer, Span<uint> output)
    {
        var temp = ArrayPool<byte>.Shared.Rent(4);
        try
        {
            var pos = 0;

            using var ms = new MemoryStream(buffer.ToArray());
            while (pos != output.Length)
            {
                ms.ReadExactly(temp, 0, 4);
                output[pos] = EndianBitConverter.ToUInt32LittleEndian(temp[..4]);
                pos += 1;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(temp, true);
        }
    }

    private static void FromUint32LittleEndian(ReadOnlySpan<uint> input, Span<byte> output)
    {
        for (var i = 0; i < input.Length; i++)
        {
            var u = input[i];
            var temp = EndianBitConverter.GetBytesLittleEndian(u);
            BinaryPrimitives.WriteUInt32LittleEndian(temp, u);
            temp.CopyTo(output[(i * 4)..]);
        }
    }
}
#endif