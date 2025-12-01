using System.Collections.Concurrent;
using Jtar.Compression.Compressor;
using Jtar.Logging;

namespace Jtar.Compression.ChunkCompressor;

public class ChunkCompressorWorker
{
    private readonly BlockingCollection<Chunk> _chunks;
    private readonly BlockingCollection<Chunk> _outputCollection;
    private readonly ICompressor _compressor;

    public ChunkCompressorWorker(BlockingCollection<Chunk> chunks, ICompressor compressor, BlockingCollection<Chunk> outputCollection)
    {
        _chunks = chunks;
        _compressor = compressor;
        _outputCollection = outputCollection;
    }

    public void Run()
    {
        while (!_chunks.IsCompleted)
        {
            try
            {
                Chunk chunk = _chunks.Take();

                var compressedData = _compressor.Compress(chunk.Data);
                Chunk compressedChunk = new Chunk(chunk.Filepath, chunk.Order, chunk.ChunkCount, compressedData);
                _outputCollection.Add(compressedChunk);
            }
            catch (InvalidOperationException)
            {
                // The collection has been marked as complete for adding and is empty.
                Logger.Log(LogType.Debug, $"ChunkCompressorWorker {Environment.CurrentManagedThreadId} interrupted and finishing");
                break;
            }
        }
    }
}