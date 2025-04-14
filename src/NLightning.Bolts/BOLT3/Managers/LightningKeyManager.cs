// using NBitcoin;
// using NBitcoin.Crypto;
//
// namespace NLightning.Bolts.BOLT3.Managers;
//
// using Common.Managers;
// using Services;
//
// public class LightningKeyManager
// {
//     private readonly KeyDerivationService _keyDerivation;
//     
//     // Node's base key pairs - long-term identity keys
//     private readonly Key _paymentBaseSecret;
//     private readonly Key _htlcBaseSecret;
//     private readonly Key _delayedPaymentBaseSecret;
//     private readonly Key _revocationBaseSecret;
//     
//     // Corresponding public keys
//     public PubKey PaymentBasepoint => _paymentBaseSecret.PubKey;
//     public PubKey HtlcBasepoint => _htlcBaseSecret.PubKey;
//     public PubKey DelayedPaymentBasepoint => _delayedPaymentBaseSecret.PubKey;
//     public PubKey RevocationBasepoint => _revocationBaseSecret.PubKey;
//     
//     // Store the remote node's base public keys
//     public PubKey RemotePaymentBasepoint { get; set; }
//     public PubKey RemoteHtlcBasepoint { get; set; }
//     public PubKey RemoteDelayedBasepoint { get; set; }
//     public PubKey RemoteRevocationBasepoint { get; set; }
//     
//     // Per-commitment secret storage
//     private readonly KeyDerivationService.PerCommitmentSecretStorage _secretStorage = 
//         new KeyDerivationService.PerCommitmentSecretStorage();
//     
//     // Initialize with node secrets or generate new ones
//     public LightningKeyManager(KeyDerivationService keyDerivation)
//     {
//         _keyDerivation = keyDerivation;
//         
//         // Generate node secrets or load from SecureKeyManager
//         byte[] nodeSeed = GenerateOrLoadNodeSeed();
//         
//         // Derive base secrets from seed
//         var hmac = new HMACSHA256(nodeSeed);
//         _paymentBaseSecret = new Key(hmac.ComputeHash(new byte[] { 0x01 }));
//         _htlcBaseSecret = new Key(hmac.ComputeHash(new byte[] { 0x02 }));
//         _delayedPaymentBaseSecret = new Key(hmac.ComputeHash(new byte[] { 0x03 }));
//         _revocationBaseSecret = new Key(hmac.ComputeHash(new byte[] { 0x04 }));
//         
//         // Initialize per-commitment seed
//         byte[] commitmentSeed = hmac.ComputeHash(new byte[] { 0x05 });
//         _keyDerivation.InitializeSecureSeed(commitmentSeed);
//     }
//     
//     private byte[] GenerateOrLoadNodeSeed()
//     {
//         try {
//             // Try to load from SecureKeyManager
//             return SecureKeyManager.GetPrivateKeyBytes();
//         }
//         catch (InvalidOperationException) {
//             // Generate new seed if not initialized
//             var seed = new byte[32];
//             using (var rng = RandomNumberGenerator.Create())
//                 rng.GetBytes(seed);
//                 
//             SecureKeyManager.Initialize(seed);
//             return seed;
//         }
//     }
//     
//     // Generate per-commitment secret for a specific commitment number
//     public byte[] GeneratePerCommitmentSecret(ulong commitmentNumber)
//     {
//         var seed = _keyDerivation.GetSecureSeed();
//         return KeyDerivationService.GeneratePerCommitmentSecret(seed, commitmentNumber);
//     }
//     
//     // Get keys for a specific commitment transaction
//     public CommitmentKeys DeriveCommitmentKeys(ulong commitmentNumber)
//     {
//         // Get per-commitment secret and point
//         byte[] perCommitmentSecret = GeneratePerCommitmentSecret(commitmentNumber);
//         PubKey perCommitmentPoint = _keyDerivation.GeneratePerCommitmentPoint(perCommitmentSecret);
//         
//         // Store the secret for later verification
//         _secretStorage.InsertSecret(perCommitmentSecret, commitmentNumber);
//         
//         // Derive all necessary keys for the commitment transaction
//         return new CommitmentKeys {
//             // Local keys
//             LocalPubkey = DerivePublicKey(PaymentBasepoint, perCommitmentPoint),
//             LocalHtlcPubkey = DerivePublicKey(HtlcBasepoint, perCommitmentPoint),
//             LocalDelayedPubkey = DerivePublicKey(DelayedPaymentBasepoint, perCommitmentPoint),
//             
//             // Remote keys
//             RemotePubkey = RemotePaymentBasepoint, // This is simply the remote's payment basepoint
//             RemoteHtlcPubkey = DerivePublicKey(RemoteHtlcBasepoint, perCommitmentPoint),
//             RemoteDelayedPubkey = DerivePublicKey(RemoteDelayedBasepoint, perCommitmentPoint),
//             
//             // Revocation key
//             RevocationPubkey = _keyDerivation.DeriveRevocationPubKey(
//                 RevocationBasepoint, perCommitmentPoint),
//                 
//             // Store commitment point for reference
//             PerCommitmentPoint = perCommitmentPoint
//         };
//     }
//     
//     // Helper method to use NBitcoin for key derivation
//     private PubKey DerivePublicKey(PubKey basepoint, PubKey perCommitmentPoint)
//     {
//         using var ms = new MemoryStream();
//         ms.Write(perCommitmentPoint.ToBytes(), 0, 33);
//         ms.Write(basepoint.ToBytes(), 0, 33);
//         
//         var hash = Hashes.SHA256(ms.ToArray());
//         
//         // In NBitcoin we need to use EC point addition: basepoint + hash*G
//         var ec = new ECKey(hash, true);
//         return basepoint.Derivation(ec);
//     }
// }