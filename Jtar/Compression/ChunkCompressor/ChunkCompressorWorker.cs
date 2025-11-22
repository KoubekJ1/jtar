using System.Collections.Concurrent;
using ZstdNet;

namespace Jtar.Compression.ChunkCompressor;

public class ChunkCompressorWorker
{
    private readonly BlockingCollection<Chunk> _chunks;
    private readonly Compressor _compressor;

    // TODO: Compression level configuration
    public ChunkCompressorWorker(BlockingCollection<Chunk> chunks)
    {
        _chunks = chunks;
        _compressor = new Compressor();
    }

    ~ChunkCompressorWorker()
    {
        _compressor.Dispose();
    }

    public void Run()
    {
        while (!_chunks.IsCompleted)
        {
            try
            {
                Chunk chunk = _chunks.Take();
                
                var compressedData = _compressor.Wrap(chunk.Data);
            }
            catch (InvalidOperationException)
            {
                // The collection has been marked as complete for adding and is empty.
                break;
            }
        }
    }
}