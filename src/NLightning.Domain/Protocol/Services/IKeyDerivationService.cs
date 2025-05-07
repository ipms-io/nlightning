using NBitcoin;

namespace NLightning.Domain.Protocol.Services;

public interface IKeyDerivationService
{
    PubKey DerivePublicKey(PubKey basepoint, PubKey perCommitmentPoint);
    Key DerivePrivateKey(Key basepointSecret, PubKey perCommitmentPoint);
    PubKey DeriveRevocationPubKey(PubKey revocationBasepoint, PubKey perCommitmentPoint);
    Key DeriveRevocationPrivKey(Key revocationBasepointSecret, Key perCommitmentSecret);
    byte[] GeneratePerCommitmentSecret(byte[] seed, ulong index);
}