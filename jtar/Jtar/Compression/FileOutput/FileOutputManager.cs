using System.Collections.Concurrent;
using Jtar.Compression.ChunkCompressor;
using Jtar.Compression.Compressor;

namespace Jtar.Compression.FileOutput;

/// <summary>
/// Manages the output of compressed file chunks to the final output stream.
/// </summary>
public class FileOutputManager
{
    public BlockingCollection<Chunk> Chunks { get; private set; }

    private readonly Stream _outputStream;
    private readonly ICompressor _compressor;

    /// <summary>
    /// Initializes a new instance of the FileOutputManager class.
    /// </summary>
    /// <param name="compressor">Compressor to use</param>
    /// <param name="outputStream">Stream to write output data to</param>
    public FileOutputManager(ICompressor compressor, Stream outputStream)
    {
        _outputStream = outputStream;
        _compressor = compressor;
        Chunks = new BlockingCollection<Chunk>(new ConcurrentQueue<Chunk>());
    }

    /// <summary>
    /// Starts the file output worker to process and write chunks to the output stream.
    /// </summary>
    /// <returns></returns>
    public async Task Run()
    {
        FileOutputWorker worker = new FileOutputWorker(Chunks, (ICompressor)_compressor.Clone(), _outputStream);
        await Task.Run(worker.Run);
        _outputStream.Close();
    }
}