namespace NLightning.Domain.Bitcoin.Interfaces;

using Crypto.ValueObjects;

public interface ISignatureValidator
{
    /// <summary>
    /// Validates a signature against protocol rules.
    /// </summary>
    bool ValidateSignature(CompactSignature signature);
}