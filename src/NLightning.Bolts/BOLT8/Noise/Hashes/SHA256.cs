using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NLightning.Bolts.BOLT8.Noise.Hashes;

/// <summary>
/// SHA-256 from <see href="https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.180-4.pdf">FIPS 180-4</see>.
/// </summary>
internal sealed class SHA256 : IDisposable
{
	private readonly IntPtr state = Marshal.AllocHGlobal(104);
	private bool disposed;

	/// <summary>
	/// A constant specifying the size in bytes of the hash output.
	/// </summary>
	public int HashLen => 32;

	/// <summary>
	/// A constant specifying the size in bytes that the hash function
	/// uses internally to divide its input for iterative processing.
	/// </summary>
	public int BlockLen => 64;

	public SHA256() => Reset();

	/// <summary>
	/// Appends the specified data to the data already processed in the hash.
	/// </summary>
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

	/// <summary>
	/// Retrieves the hash for the accumulated data into the hash parameter,
	/// and resets the object to its initial state.
	/// </summary>
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