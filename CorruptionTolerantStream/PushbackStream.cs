namespace CorruptionTolerantStream;

internal class PushbackStream(Stream inner) : Stream
{
    private MemoryStream _pushback = new();

    public override bool CanRead => inner.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var totalRead = 0;
        var pushbackAvailable = (int)(_pushback.Length - _pushback.Position);
        if (pushbackAvailable > 0)
        {
            var toReadFromPushback = Math.Min(count, pushbackAvailable);
            var readFromPushback = _pushback.Read(buffer, offset, toReadFromPushback);
            totalRead += readFromPushback;
            offset += readFromPushback;
            count -= readFromPushback;
        }
        if (count > 0)
        {
            var readFromInner = inner.Read(buffer, offset, count);
            totalRead += readFromInner;
        }
        return totalRead;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var totalRead = 0;
        var pushbackAvailable = (int)(_pushback.Length - _pushback.Position);
        if (pushbackAvailable > 0)
        {
            var toReadFromPushback = Math.Min(buffer.Length, pushbackAvailable);
            var readFromPushback = _pushback.Read(buffer.Span[..toReadFromPushback]);
            totalRead += readFromPushback;
            buffer = buffer[readFromPushback..];
        }
        if (buffer.Length > 0)
        {
            var readFromInner = await inner.ReadAsync(buffer, cancellationToken);
            totalRead += readFromInner;
        }
        return totalRead;
    }

    public void Unread(ReadOnlySpan<byte> buffer)
    {
        // Optimize for the common case where pushback buffer is exhausted
        if (_pushback.Position == _pushback.Length)
        {
            _pushback.Position = _pushback.Length;
            _pushback.Write(buffer);
            _pushback.Position = _pushback.Length - buffer.Length;
            return;
        }
        
        // General case: need to prepend the buffer
        var auxiliary = new MemoryStream(buffer.Length + (int)(_pushback.Length - _pushback.Position));
        auxiliary.Write(buffer);
        _pushback.CopyTo(auxiliary);
        _pushback.Dispose();
        _pushback = auxiliary;
        _pushback.Position = 0;
    }

    public override long Seek(long offset, SeekOrigin origin) =>
        throw new NotSupportedException();

    public override void SetLength(long value) =>
        throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();

    public override void Flush() =>
        throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pushback.Dispose();
            inner.Dispose();
        }
        base.Dispose(disposing);
    }
}
