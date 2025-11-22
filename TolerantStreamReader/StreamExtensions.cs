namespace TolerantStreamReader;

using System.IO.Hashing;

public static class StreamExtensions
{
    public static async Task WriteFramed(this Stream stream, byte[] payload) =>
        await stream.WriteFramed(Constants.Magic, payload);

    public static async Task WriteFramed(this Stream stream, byte[] magic, byte[] payload)
    {
        var size = BitConverter.GetBytes(payload.Length);
        var sizeHash = Crc32.HashToUInt32(size);
        await stream.WriteAsync(magic);
        await stream.WriteAsync(size);
        await stream.WriteAsync(BitConverter.GetBytes(sizeHash));
        await stream.WriteAsync(payload);
        var payloadHash = Crc32.HashToUInt32(payload);
        await stream.WriteAsync(BitConverter.GetBytes(payloadHash));
    }
}
