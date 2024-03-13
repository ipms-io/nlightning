using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NLightning.Bolts.BOLT8.Noise.Hashes;

using Interfaces;

/// <summary>
/// SHA-256 from <see href="https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.180-4.pdf">FIPS 180-4</see>.
/// </summary>
internal sealed class SHA256 : IHash
{
	private readonly IntPtr state = Marshal.AllocHGlobal(104);
	private bool disposed;

	public SHA256() => Reset();

	public int HashLen => 32;
	public int BlockLen => 64;

	public void AppendData(ReadOnlySpan<byte> data)
	{
		if (!data.IsEmpty)
		{
			_ = Libsodium.crypto_hash_sha256_update(
				state,
				ref MemoryMarshal.GetReference(data),
				(ulong)data.Length
			);
		}
	}

	public void GetHashAndReset(Span<byte> hash)
	{
		Debug.Assert(hash.Length == HashLen);

		_ = Libsodium.crypto_hash_sha256_final(
			state,
			ref MemoryMarshal.GetReference(hash)
		);

		Reset();
	}

	private void Reset()
	{
		_ = Libsodium.crypto_hash_sha256_init(state);
	}

	public void Dispose()
	{
		if (!disposed)
		{
			Marshal.FreeHGlobal(state);
			disposed = true;
		}
	}
}