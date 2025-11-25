using BenchmarkDotNet.Running;
using CorruptionTolerantStream.Benchmarks;

BenchmarkRunner.Run<CorruptionTolerantReaderBenchmark>();
