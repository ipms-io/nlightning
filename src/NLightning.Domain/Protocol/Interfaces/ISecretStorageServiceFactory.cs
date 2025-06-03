using NLightning.Domain.Protocol.Services;

namespace NLightning.Domain.Protocol.Interfaces;

public interface ISecretStorageServiceFactory
{
    ISecretStorageService CreatePerCommitmentStorage();
}