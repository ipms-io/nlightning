namespace NLightning.Domain.ValueObjects;

public class StoredSecret(ulong index, IntPtr secretPtr)
{
    public ulong Index { get; } = index;
    public IntPtr SecretPtr { get; } = secretPtr;
}