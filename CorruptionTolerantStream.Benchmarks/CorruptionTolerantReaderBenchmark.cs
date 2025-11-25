namespace CorruptionTolerantStream.Benchmarks;

using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;

public class CorruptionTolerantReaderBenchmark
{
    private readonly IImmutableList<byte[]> _payloads = CreatePayloads().ToImmutableList();

    [Benchmark]
    public async Task WriteNotFramed()
    {
        using var stream = new MemoryStream();
        foreach (var payload in _payloads)
        {
            await stream.WriteAsync(payload);
        }
    }

    [Benchmark]
    public async Task WriteFramed()
    {
        using var stream = new MemoryStream();
        foreach (var payload in _payloads)
        {
            await stream.WritePayload(payload);
        }
    }

    private static IEnumerable<byte[]> CreatePayloads()
    {
        for (var i = 0; i < 700; i++)
        {
            yield return Enumerable.Range(0, 1024 * 1024).Select(x => (byte)x).ToArray();
        }
    }
}
