using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task TestAsync()
    {
        var reader = new StringReader("test");
        reader.ReadToEnd();
    }
}
