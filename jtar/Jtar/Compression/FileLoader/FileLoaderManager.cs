using System.Collections.Concurrent;
using Jtar.Compression.ChunkCompressor;

namespace Jtar.Compression.FileLoader;

public class FileLoaderManager
{
    public BlockingCollection<string> Filepaths { get; } = new BlockingCollection<string>(new ConcurrentQueue<string>());

    private readonly LinkedList<Thread> _workerThreads = new LinkedList<Thread>();
    private readonly BlockingCollection<Chunk> _outputCollection;

    public FileLoaderManager(BlockingCollection<Chunk> outputCollection)
    {
        _outputCollection = outputCollection;
    }

    public async Task Run()
    {
        var worker = new FileLoaderWorker(Filepaths, _outputCollection);
        await Task.Run(worker.Run);
        _outputCollection.CompleteAdding();
    }
}