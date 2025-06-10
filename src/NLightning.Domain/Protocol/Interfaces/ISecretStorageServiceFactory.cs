namespace NLightning.Domain.Protocol.Interfaces;

public interface ISecretStorageServiceFactory
{
    ISecretStorageService CreatePerCommitmentStorage();
}