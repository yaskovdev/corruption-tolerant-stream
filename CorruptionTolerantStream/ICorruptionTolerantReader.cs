namespace CorruptionTolerantStream;

public interface ICorruptionTolerantReader
{
    Task<ReadResult> ReadPayload(CancellationToken cancellationToken);
}
