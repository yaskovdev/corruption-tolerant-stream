namespace TolerantStreamReader;

public interface IStreamReader
{
    Task<byte[]> ReadNext(CancellationToken cancellationToken);
}
