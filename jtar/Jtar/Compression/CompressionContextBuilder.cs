using Jtar.Compression.Compressor;
using Jtar.Compression.FileLoader;

namespace Jtar.Compression;

public class CompressionContextBuilder
{
    private int _threadCount = Environment.ProcessorCount - 1;
    private IEnumerable<string> _inputFiles = Array.Empty<string>();
    private string _outputFile = "output.tar.zstd";
    private ICompressor _compressor = new NoCompressor();
    private ITarFormatter _tarFormatter = new PaxTarFormatter();

    public CompressionContextBuilder SetThreadCount(int threadCount)
    {
        _threadCount = threadCount;
        return this;
    }

    public CompressionContextBuilder SetInputFiles(IEnumerable<string> inputFiles)
    {
        _inputFiles = inputFiles;
        return this;
    }

    public CompressionContextBuilder SetOutputFile(string outputFile)
    {
        _outputFile = outputFile;
        return this;
    }

    public CompressionContextBuilder SetCompressor(ICompressor compressor)
    {
        _compressor = compressor;
        return this;
    }

    public CompressionContextBuilder SetTarFormatter(ITarFormatter tarFormatter)
    {
        _tarFormatter = tarFormatter;
        return this;
    }

    public CompressionContext Build()
    {
        return new CompressionContext(_threadCount, _inputFiles, _outputFile, _compressor, _tarFormatter);
    }
}