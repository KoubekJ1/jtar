using Jtar.Compression.ChunkCompressor;
using Jtar.Compression.FileLoader;
using Jtar.Compression.FileSeeker;
using Jtar.Logging;

namespace Jtar.Compression;

public class CompressionContext
{
    private readonly int _threadCount;
    private readonly IEnumerable<string> _inputFiles;
    private readonly string _outputFile;

    private readonly ChunkCompressorManager _chunkCompressorManager;
    private readonly FileLoaderManager _fileLoaderManager;
    private readonly FileSeekerManager _fileSeekerManager;

    public CompressionContext(int threadCount, IEnumerable<string> inputFiles, string outputFile)
    {
        _threadCount = threadCount;
        _inputFiles = inputFiles;
        _outputFile = outputFile;

        _chunkCompressorManager = new ChunkCompressorManager(Math.Max(1, threadCount-3));
        _fileLoaderManager = new FileLoaderManager(_chunkCompressorManager.Chunks);
        _fileSeekerManager = new FileSeekerManager(_inputFiles, _fileLoaderManager.Filepaths);
    }

    public void Compress()
    {
        Logger.Log(LogType.Debug, "Starting compression process...");
        _fileSeekerManager.Run();
        _fileLoaderManager.Run();
        _chunkCompressorManager.Run();
    }
}