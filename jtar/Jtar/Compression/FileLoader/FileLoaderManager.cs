using System.Collections.Concurrent;
using Jtar.Compression.ChunkCompressor;

namespace Jtar.Compression.FileLoader;

/// <summary>
/// Manages file loading and TAR formatting using provided file paths.
/// The loaded files are processed into chunks and added to the output collection.
/// </summary>
public class FileLoaderManager
{
    public BlockingCollection<string> Filepaths { get; } = new BlockingCollection<string>(new ConcurrentQueue<string>());
    private readonly BlockingCollection<Chunk> _outputCollection;

    /// <summary>
    /// Initializes a new instance of the FileLoaderManager class.
    /// </summary>
    /// <param name="outputCollection">Collection to add loaded chunks to</param>
    public FileLoaderManager(BlockingCollection<Chunk> outputCollection)
    {
        _outputCollection = outputCollection;
    }

    /// <summary>
    /// Starts the file loading process.
    /// </summary>
    public async Task Run()
    {
        var worker = new FileLoaderWorker(Filepaths, _outputCollection);
        await Task.Run(worker.Run);
        _outputCollection.CompleteAdding();
    }
}