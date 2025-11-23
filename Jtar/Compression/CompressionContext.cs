using Jtar.Compression.ChunkCompressor;
using Jtar.Compression.Compressor;
using Jtar.Compression.FileLoader;
using Jtar.Compression.FileOutput;
using Jtar.Compression.FileSeeker;
using Jtar.Logging;

namespace Jtar.Compression;

public class CompressionContext
{
    private readonly int _threadCount;
    private readonly IEnumerable<string> _inputFiles;
    private readonly string _outputFile;

    private readonly FileOutputManager _fileOutputManager;
    private readonly ChunkCompressorManager _chunkCompressorManager;
    private readonly FileLoaderManager _fileLoaderManager;
    private readonly FileSeekerManager _fileSeekerManager;

    public CompressionContext(int threadCount, IEnumerable<string> inputFiles, string outputFile, ICompressor? compressor = null)
    {
        _threadCount = threadCount;
        _inputFiles = inputFiles;
        _outputFile = outputFile;

        compressor = compressor ?? new NoCompressor();
    
        _fileOutputManager = new FileOutputManager(compressor, new FileStream(_outputFile, FileMode.Create, FileAccess.Write));
        _chunkCompressorManager = new ChunkCompressorManager(Math.Max(1, threadCount-3), compressor, _fileOutputManager.Chunks);
        _fileLoaderManager = new FileLoaderManager(_chunkCompressorManager.Chunks);
        _fileSeekerManager = new FileSeekerManager(_inputFiles, _fileLoaderManager.Filepaths);
    }

    public void Compress()
    {
        Logger.Log(LogType.Debug, "Starting compression process...");
        _fileSeekerManager.Run();
        _fileLoaderManager.Run();
        _chunkCompressorManager.Run();
        _fileOutputManager.Run();
    }
}