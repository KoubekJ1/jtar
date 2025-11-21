namespace Jtar.Compression;

public class CompressionContext
{
    public readonly int _threadCount;
    private readonly IEnumerable<string> _inputFiles;
    public readonly string _outputFile;

    public CompressionContext(int threadCount, IEnumerable<string> inputFiles, string outputFile)
    {
        _threadCount = threadCount;
        _inputFiles = inputFiles;
        _outputFile = outputFile;
    }

    public void Compress()
    {
        throw new NotImplementedException();
    }
}