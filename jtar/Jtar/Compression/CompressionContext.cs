using Jtar.Compression.ChunkCompressor;
using Jtar.Compression.Compressor;
using Jtar.Compression.FileLoader;
using Jtar.Compression.FileOutput;
using Jtar.Compression.FileSeeker;
using Jtar.Exceptions;
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
    private readonly ITarFormatter _tarFormatter;

    /// <summary>
    /// Initializes a new instance of the CompressionContext class.
    /// </summary>
    /// <param name="threadCount">Amount of threads used</param>
    /// <param name="inputFiles">Files/Directories to be compressed</param>
    /// <param name="outputFile">Output file path</param>
    /// <param name="compressor">Compressor to use for compressing chunks</param>
    /// <exception cref="InvalidOutputFileException"></exception>
    public CompressionContext(int threadCount, IEnumerable<string> inputFiles, string outputFile, ICompressor? compressor = null, ITarFormatter? tarFormatter = null)
    {
        _threadCount = threadCount;
        _inputFiles = inputFiles;
        _outputFile = outputFile;

        compressor = compressor ?? new NoCompressor();
        _tarFormatter = tarFormatter ?? new PaxTarFormatter();
    
        if (Directory.Exists(outputFile))
        {
            throw new InvalidOutputFileException("Output file path is an existing directory!");
        }
        if (!Directory.Exists(Directory.GetParent(_outputFile)?.ToString()))
        {
            throw new InvalidOutputFileException("Output file directory does not exist!");
        }
        _fileOutputManager = new FileOutputManager(compressor, new FileStream(_outputFile, FileMode.Create, FileAccess.Write));
        _chunkCompressorManager = new ChunkCompressorManager(Math.Max(1, threadCount-3), compressor, _fileOutputManager.Chunks);
        _fileLoaderManager = new FileLoaderManager(_chunkCompressorManager.Chunks);
        _fileSeekerManager = new FileSeekerManager(_inputFiles, _fileLoaderManager.Filepaths);
    }

    /// <summary>
    /// Starts the compression process.
    /// </summary>
    public async Task Compress()
    {
        Logger.Log(LogType.Debug, "Starting compression process...");

        LinkedList<Task> tasks = new LinkedList<Task>();
        tasks.AddLast(Task.Run(_fileSeekerManager.Run));
        tasks.AddLast(Task.Run(_fileLoaderManager.Run));
        tasks.AddLast(Task.Run(_chunkCompressorManager.Run));
        tasks.AddLast(Task.Run(_fileOutputManager.Run));

        await Task.WhenAll(tasks);
    }
}