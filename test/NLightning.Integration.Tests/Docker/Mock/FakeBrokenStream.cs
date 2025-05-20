namespace NLightning.Integration.Tests.Docker.Mock;

public class FakeBrokenStream : Stream
{
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException("Length is not supported.");

    public override long Position
    {
        get => throw new NotSupportedException("Position is not supported.");
        set => throw new NotSupportedException("Position is not supported.");
    }

    public override void Flush()
    {
        throw new NotSupportedException("Flush is not supported.");
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        // Simulate an IO exception when attempting to read
        throw new IOException("Simulated read error.");
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException("Seek is not supported.");
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException("SetLength is not supported.");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("Write is not supported.");
    }
}