namespace TolerantStreamReader.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

[TestClass]
public class PushbackStreamTests
{
    [TestMethod]
    public async Task Should2()
    {
        using var inner = new MemoryStream(Enumerable.Range(0, 6).Select(i => (byte)i).ToArray());
        var instanceUnderTest = new PushbackStream(inner);
        await ReadAndAssertExpectedBytes(instanceUnderTest, 3, [0, 1, 2]);
        await ReadAndAssertExpectedBytes(instanceUnderTest, 1, [3]);
        instanceUnderTest.Unread([2, 3]);
        instanceUnderTest.Unread([0, 1]);
        await ReadAndAssertExpectedBytes(instanceUnderTest, 0, []);
        await ReadAndAssertExpectedBytes(instanceUnderTest, 3, [0, 1, 2]);
        await ReadAndAssertExpectedBytes(instanceUnderTest, 2, [3]);
        await ReadAndAssertExpectedBytes(instanceUnderTest, 2, [4, 5]);
    }

    private static async Task ReadAndAssertExpectedBytes(Stream stream, int bytesToRead, byte[] expected)
    {
        var buffer = new Memory<byte>(new byte[bytesToRead]);
        var count = await stream.ReadAsync(buffer);
        count.ShouldBe(expected.Length);
        var array = buffer.Span[..count].ToArray();
        array.ShouldBe(expected);
    }
}
