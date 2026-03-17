using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class Example
{
    public async Task TestAsync()
    {
        var reader = new StringReader("test");

        // SHARPEN003 / SHARPEN009: sync call with async equivalent.
        reader.ReadToEnd();

        // SHARPEN004: Thread.Sleep -> await Task.Delay
        Thread.Sleep(10);

        // SHARPEN005: Task<T>.Result -> await
        var t = Task.FromResult(42);
        var x = t.Result;

        // SHARPEN006: Task.Wait() -> await
        Task.Delay(1).Wait();

        // SHARPEN007: Task.WaitAny(...) -> await Task.WhenAny(...)
        Task.WaitAny(Task.Delay(1), Task.Delay(2));

        // SHARPEN008: Task.WaitAll(...) -> await Task.WhenAll(...)
        Task.WaitAll(Task.Delay(1), Task.Delay(2));
    }
}