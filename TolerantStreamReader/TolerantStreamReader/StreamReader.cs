namespace TolerantStreamReader;

using System.IO.Hashing;
using LanguageExt;

public class StreamReader(Stream stream, TimeSpan delayBetweenReadRetries) : IStreamReader
{
    private static readonly byte[] Magic = [0xDE, 0xAD, 0xBE, 0xEF];
    private readonly PushbackStream _stream = new(stream);

    public async Task<byte[]> ReadNext(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ReadMagicInternal(Magic, cancellationToken);
            using var sizeBuffer = await ReadFromStreamExact(sizeof(int), cancellationToken);
            var size = BitConverter.ToInt32(sizeBuffer.Memory.Span);
            using var payload = await ReadFromStreamExact(size, cancellationToken);
            using var expectedHashBuffer = await ReadFromStreamExact(sizeof(uint), cancellationToken);
            var expectedHash = BitConverter.ToUInt32(expectedHashBuffer.Memory.Span);
            var actualHash = Crc32.HashToUInt32(payload.Memory.Span);
            if (expectedHash == actualHash)
            {
                return payload.Memory.ToArray();
            }
            _stream.Unread(expectedHashBuffer.Memory.ToArray());
            _stream.Unread(payload.Memory.ToArray());
            _stream.Unread(sizeBuffer.Memory.ToArray());
            await ReadFromStreamExact(1, cancellationToken);
        }
    }

    /// <summary>
    /// Reads the specified magic byte sequence from the stream, advancing until the full sequence is matched.
    /// </summary>
    private async Task<Unit> ReadMagicInternal(byte[] magic, CancellationToken cancellationToken)
    {
        var matchedMagicBytes = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var buffer = await ReadFromStreamExact(1, cancellationToken);

            var b = buffer.Memory.Span[0];

            if (b == magic[matchedMagicBytes])
            {
                matchedMagicBytes++;

                if (matchedMagicBytes == magic.Length)
                {
                    return Prelude.unit;
                }
            }
            else
            {
                matchedMagicBytes = b == magic[0] ? 1 : 0;
            }
        }
    }

    private async Task<ReadBuffer> ReadFromStreamExact(int size, CancellationToken cancellationToken)
    {
        var buffer = new ReadBuffer(size);
        var totalRead = 0;
        while (totalRead < size)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = await _stream.ReadAsync(buffer.Memory[totalRead..], cancellationToken);
            totalRead += read;
            if (read == 0)
            {
                await Task.Delay(delayBetweenReadRetries, cancellationToken);
            }
        }

        return buffer;
    }
}
