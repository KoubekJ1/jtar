using Jtar.Logging;

namespace Jtar;

class Program
{
    static void Main(string[] args)
    {
        Logger.ShowDebugMessages = true;
        // TODO: Block absolute paths
        var context = new Compression.CompressionContext(
            4,
            new List<string> { "obj" },
            "output.tar"
        );

        context.Compress();
    }
}
