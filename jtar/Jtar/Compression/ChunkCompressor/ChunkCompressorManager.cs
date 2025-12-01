using System.Collections.Concurrent;
using Jtar.Compression.Compressor;

namespace Jtar.Compression.ChunkCompressor;

public class ChunkCompressorManager
{
    public BlockingCollection<Chunk> Chunks { get; private set; }
    private readonly int _workerCount;
    private readonly BlockingCollection<Chunk> _outputCollection;
    private readonly ICompressor _compressor;

    public ChunkCompressorManager(int workerCount, ICompressor compressor, BlockingCollection<Chunk> outputCollection)
    {
        Chunks = new BlockingCollection<Chunk>(new ConcurrentQueue<Chunk>(), 800); // 800 chunks ... 100 MB
        _workerCount = workerCount;
        _outputCollection = outputCollection;
        _compressor = compressor;
    }

    public async Task Run()
    {
        Task[] tasks = new Task[_workerCount];
        for (int i = 0; i < _workerCount; i++)
        {
            ChunkCompressorWorker worker = new ChunkCompressorWorker(Chunks, (ICompressor)_compressor.Clone(), _outputCollection);
            Task thread = Task.Run(worker.Run);
            tasks[i] = thread;
        }
        await Task.WhenAll(tasks);
        _outputCollection.CompleteAdding();
    }
}