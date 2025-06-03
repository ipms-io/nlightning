using NLightning.Domain.Crypto.ValueObjects;

namespace NLightning.Domain.Bitcoin.Interfaces;

public interface ISignatureValidator
{
    /// <summary>
    /// Validates a signature against protocol rules.
    /// </summary>
    bool ValidateSignature(DerSignature signature);
}