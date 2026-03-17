using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sharpen.Analyzer.Sample;

public static class CSharp5Samples
{
    public static async Task AwaitTaskDelayInsteadOfCallingThreadSleepAsync()
    {
        // SHARPEN004
        Thread.Sleep(100);
    }

    public static async Task AwaitTaskInsteadOfCallingTaskResultAsync()
    {
        // SHARPEN005
        var task = Task.FromResult(42);
        var value = task.Result;

        Console.WriteLine(value);
    }

    public static async Task AwaitTaskInsteadOfCallingTaskWaitAsync()
    {
        // SHARPEN006
        Task.Delay(10).Wait();
    }

    public static async Task AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyAsync()
    {
        // SHARPEN007
        Task.WaitAny(Task.Delay(10), Task.Delay(20));
    }

    public static async Task AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAsync()
    {
        // SHARPEN008
        Task.WaitAll(Task.Delay(10), Task.Delay(20));
    }

    public static async Task ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronousAsync(
        Stream stream)
    {
        // SHARPEN003 / SHARPEN009
        stream.Read(new byte[1], 0, 1);
    }
}