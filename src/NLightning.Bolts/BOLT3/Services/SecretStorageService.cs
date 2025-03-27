// namespace NLightning.Bolts.BOLT3.Services;
//
// public class SecretStorageService
// {
//     public class PerCommitmentSecretStorage
//     {
//         private class StoredSecret
//         {
//             public ulong Index { get; set; }
//             public byte[] Secret { get; set; }
//         }
//         
//         private readonly StoredSecret[] _knownSecrets = new StoredSecret[49];
//         
//         /// <summary>
//         /// Inserts a new secret and verifies it against existing secrets
//         /// </summary>
//         public bool InsertSecret(byte[] secret, ulong index)
//         {
//             int bucket = GetBucketIndex(index);
//             
//             // Verify this secret can derive all previously known secrets
//             for (int b = 0; b < bucket; b++)
//             {
//                 if (_knownSecrets[b] != null)
//                 {
//                     byte[] derived = DeriveSecret(secret, bucket, _knownSecrets[b].Index);
//                     if (!derived.SequenceEqual(_knownSecrets[b].Secret))
//                     {
//                         return false; // Secret verification failed
//                     }
//                 }
//             }
//             
//             _knownSecrets[bucket] = new StoredSecret { Index = index, Secret = secret };
//             return true;
//         }
//         
//         /// <summary>
//         /// Derives an old secret from a known higher-level secret
//         /// </summary>
//         public byte[] DeriveOldSecret(ulong index)
//         {
//             for (int b = 0; b < _knownSecrets.Length; b++)
//             {
//                 if (_knownSecrets[b] != null)
//                 {
//                     // Create mask with b trailing zeros
//                     ulong mask = ~((1UL << b) - 1);
//                     
//                     if ((_knownSecrets[b].Index & mask) == (index & mask))
//                     {
//                         return DeriveSecret(_knownSecrets[b].Secret, b, index);
//                     }
//                 }
//             }
//             
//             throw new InvalidOperationException($"Secret at index {index} cannot be derived from known secrets");
//         }
//         
//         private int GetBucketIndex(ulong index)
//         {
//             for (int b = 0; b < 48; b++)
//             {
//                 if (((index >> b) & 1) == 1)
//                 {
//                     return b;
//                 }
//             }
//             return 48; // For index 0 (seed)
//         }
//         
//         private byte[] DeriveSecret(byte[] baseSecret, int bits, ulong index)
//         {
//             byte[] result = new byte[baseSecret.Length];
//             Buffer.BlockCopy(baseSecret, 0, result, 0, baseSecret.Length);
//             
//             for (int b = bits - 1; b >= 0; b--)
//             {
//                 if (((index >> b) & 1) != 0)
//                 {
//                     result[b / 8] ^= (byte)(1 << (b % 8));
//                     result = Hashes.SHA256(result);
//                 }
//             }
//             
//             return result;
//         }
//     }
// }