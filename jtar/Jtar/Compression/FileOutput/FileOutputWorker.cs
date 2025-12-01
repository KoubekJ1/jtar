using System.Collections.Concurrent;
using Jtar.Compression.ChunkCompressor;
using Jtar.Compression.Compressor;
using Jtar.Logging;

namespace Jtar.Compression.FileOutput;

public class FileOutputWorker
{
    private readonly BlockingCollection<Chunk> _chunks;
    private readonly Stream _outputStream;

    private readonly Dictionary<string, List<Chunk>> _fileChunks = new Dictionary<string, List<Chunk>>();
    private readonly ICompressor _compressor;

    public FileOutputWorker(BlockingCollection<Chunk> chunks, ICompressor compressor, Stream outputStream)
    {
        _chunks = chunks;
        _compressor = compressor;
        _outputStream = outputStream;
    }

    public void Run()
    {
        // TODO: Write file exactly where it belongs in the final file
        while (!_chunks.IsCompleted || _fileChunks.Count > 0)
        {
            try
            {
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
                chunkList.Add(newChunk);
                if (chunkList.Count >= newChunk.ChunkCount)
                {
                    WriteChunks(chunkList);
                    _fileChunks.Remove(newChunk.Filepath);
                }
            }
            catch (InvalidOperationException)
            {
                Logger.Log(LogType.Debug, $"FileOutputWorker {Environment.CurrentManagedThreadId} interrupted and finishing");
                break;
            }
        }

        // Write final empty block to signify end of TAR archive as per the specification
        byte[] endBlock = new byte[1024];
        
        byte[] compressedEnd = _compressor.Compress(endBlock);
        Logger.Log(LogType.Debug, "CompressedEnd Length: " + compressedEnd.Length);

        _outputStream.Write(compressedEnd, 0, compressedEnd.Length);
    }

    private void WriteChunks(List<Chunk> chunks)
    {
        Logger.Log(LogType.Debug, $"FileOutputWorker {Environment.CurrentManagedThreadId} writing file: " + chunks.First().Filepath);
        foreach (var chunk in chunks.OrderBy(x => x.Order))
        {
            //Logger.Log(LogType.Debug, $"FileOutputWorker {Environment.CurrentManagedThreadId} writing data {string.Join(",", chunk.Data)}");
            _outputStream.Write(chunk.Data, 0, chunk.Data.Length);
        }
    }
}