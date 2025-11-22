namespace TolerantStreamReader;

using LanguageExt;

public interface IStreamReader
{
    Aff<ReadResult> ReadNext(CancellationToken cancellationToken);
}
