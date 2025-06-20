namespace NLightning.Domain.Utils;

public static class ExceptionUtils
{
    public static void ThrowIfDisposed(bool disposed, string name)
    {
        if (!disposed)
        {
            return;
        }

        throw new ObjectDisposedException(name);
    }
}