using Jtar.Logging;

namespace Jtar;

class Program
{
    static void Main(string[] args)
    {
        Logger.ShowDebugMessages = true;
        // TODO: Block absolute paths
        
        Compression.CompressionContextBuilder builder = new Compression.CompressionContextBuilder();
        Compression.CompressionContext context = builder
            .SetInputFiles(new string[] { "obj" })
            .SetOutputFile("output.tar")
            .SetThreadCount(11)
            .Build();

        context.Compress();
    }
}
