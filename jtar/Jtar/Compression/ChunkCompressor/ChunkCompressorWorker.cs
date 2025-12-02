using System.Collections.Concurrent;
using Jtar.Compression.Compressor;
using Jtar.Logging;

namespace Jtar.Compression.ChunkCompressor;

/// <summary>
/// Worker that compresses chunks using a specified compressor and passes them to an output collection.
/// Chunks are read from the input collection and compressed in a loop into the output collection until the input collection is marked as complete.
/// </summary>
public class ChunkCompressorWorker
{
    private readonly BlockingCollection<Chunk> _chunks;
    private readonly BlockingCollection<Chunk> _outputCollection;
    private readonly ICompressor _compressor;

    /// <summary>
    /// Initializes a new instance of the ChunkCompressorWorker class.
    /// </summary>
    /// <param name="chunks">Chunk collection to read from</param>
    /// <param name="compressor">Compressor to compress chunks with</param>
    /// <param name="outputCollection">Output collection to write compressed chunks to</param>
    public ChunkCompressorWorker(BlockingCollection<Chunk> chunks, ICompressor compressor, BlockingCollection<Chunk> outputCollection)
    {
        _chunks = chunks;
        _compressor = compressor;
        _outputCollection = outputCollection;
    }

    /// <summary>
    /// Runs the chunk compression worker.
    /// </summary>
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