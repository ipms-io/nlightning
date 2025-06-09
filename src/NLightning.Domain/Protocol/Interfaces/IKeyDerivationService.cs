namespace NLightning.Domain.Protocol.Interfaces;

using Crypto.ValueObjects;

public interface IKeyDerivationService
{
    CompactPubKey DerivePublicKey(CompactPubKey compactBasepoint, CompactPubKey compactPerCommitmentPoint);
    PrivKey DerivePrivateKey(PrivKey basepointSecretPriv, CompactPubKey compactPerCommitmentPoint);

    CompactPubKey DeriveRevocationPubKey(CompactPubKey compactRevocationBasepoint,
                                         CompactPubKey compactPerCommitmentPoint);

    PrivKey DeriveRevocationPrivKey(PrivKey revocationBasepointSecretPriv, PrivKey perCommitmentSecretPriv);
    Secret GeneratePerCommitmentSecret(Secret seed, ulong index);
}