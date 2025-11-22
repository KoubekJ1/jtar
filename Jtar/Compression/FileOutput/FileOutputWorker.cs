using System.Collections.Concurrent;
using Jtar.Compression.ChunkCompressor;
using ZstdNet;

namespace Jtar.Compression.FileOutput;

public class FileOutputWorker
{
    private readonly BlockingCollection<Chunk> _chunks;
    private readonly FileStream _outputStream;

    private readonly Dictionary<string, List<Chunk>> _fileChunks = new Dictionary<string, List<Chunk>>();

    public FileOutputWorker(BlockingCollection<Chunk> chunks, string outputFilepath)
    {
        _chunks = chunks;
        _outputStream = new FileStream(outputFilepath, FileMode.Create, FileAccess.Write);
    }

    ~FileOutputWorker()
    {
        // Write final empty block to signify end of TAR archive as per the specification
        byte[] endBlock = new byte[1024];

        // TODO: Implement compressor abstaction
        using var compressor = new Compressor();
        byte[] compressedEnd = compressor.Wrap(endBlock);

        _outputStream.Write(compressedEnd, 0, compressedEnd.Length);

        _outputStream.Close();
    }

    public void Run()
    {
        // TODO: Write file exactly where it belongs in the final file
        while (!_chunks.IsCompleted || _fileChunks.Count > 0)
        {
            var completeChunkList = PopCompleteList();
            if (completeChunkList != null)
            {
                WriteChunks(completeChunkList);
                continue;
            }

            var newChunk = _chunks.Take();
            List<Chunk> chunkList;
            if (_fileChunks.ContainsKey(newChunk.Filepath))
            {
                chunkList = _fileChunks[newChunk.Filepath];
                
                // Check object integrity
                var expectedChunkCount = chunkList.First().ChunkCount;
                if (newChunk.ChunkCount != expectedChunkCount)
                {
                    throw new InvalidDataException($"Chunk count mismatch for file {newChunk.Filepath}: expected {expectedChunkCount}, got {newChunk.ChunkCount}");
                }
            }
            else
            {
                chunkList = new List<Chunk>(newChunk.ChunkCount);
                _fileChunks.Add(newChunk.Filepath, chunkList);
            }
            chunkList.Insert(newChunk.Order, newChunk);
        }
    }

    private List<Chunk>? PopCompleteList()
    {
        foreach (var pair in _fileChunks)
        {
            var chunkList = pair.Value;
            if (chunkList.Count >= chunkList.First().ChunkCount)
            {
                _fileChunks.Remove(pair.Key);
                return chunkList;
            }
        }
        return null;
    }

    private void WriteChunks(List<Chunk> chunks)
    {
        foreach (var chunk in chunks)
        {
            _outputStream.Write(chunk.Data, 0, chunk.Data.Length);
        }
    }
}