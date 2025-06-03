using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Crypto;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Money;

namespace NLightning.Domain.Bitcoin.Interfaces
{
    /// <summary>
    /// Interface for transaction signing services that can be implemented either locally 
    /// or delegated to external services like VLS (Validating Lightning Signer)
    /// </summary>
    public interface ILightningSigner
    {
        /// <summary>
        /// Signs a funding transaction
        /// </summary>
        /// <param name="feeRatePerKw">The fee rate per kw to use for calculating fees</param>
        /// <returns>The signed transaction</returns>
        SignedTransaction SignTransaction(SignedTransaction unsignedTx, LightningMoney feeRatePerKw);
            
        /// <summary>
        /// Verifies a signature for a given transaction
        /// </summary>
        /// <param name="transaction">The transaction to verify</param>
        /// <param name="derSignature">The signature to verify</param>
        /// <param name="pubKey">The public key of the signer</param>
        /// <param name="index">The index of the input being verified</param>
        /// <param name="amount">The amount of the input being spent</param>
        /// <param name="redeemScript">The redeem script if applicable</param>
        /// <returns>True if the signature is valid, false otherwise</returns>
        bool VerifySignature(
            SignedTransaction transaction, 
            DerSignature derSignature, 
            CompactPubKey pubKey, 
            int index, 
            LightningMoney amount,
            BitcoinScript? redeemScript = null);
            
        /// <summary>
        /// Generates and returns the public key that will be used for key derivation
        /// </summary>
        /// <param name="keyIdentifier">Optional identifier for the key</param>
        /// <returns>The public key</returns>
        CompactPubKey GetPublicKey(string? keyIdentifier = null);
        
        /// <summary>
        /// Generates a new random key for channel operations
        /// </summary>
        /// <returns>The public key of the newly generated key</returns>
        CompactPubKey GenerateChannelKey();
        
        /// <summary>
        /// Derives the per-commitment point for a given commitment number
        /// </summary>
        /// <param name="commitmentNumber">The commitment number</param>
        /// <returns>The per-commitment point (public key)</returns>
        CompactPubKey DerivePerCommitmentPoint(ulong commitmentNumber);

        bool ValidateSignature(DerSignature payloadSignature);
    }
}