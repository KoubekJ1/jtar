using System.Collections.Concurrent;
using Jtar.Compression.ChunkCompressor;

namespace Jtar.Compression.FileOutput;

public class FileOutputManager
{
    public BlockingCollection<Chunk> Chunks { get; private set; }

    private readonly string _outputFilepath;

    public FileOutputManager(string outputFilepath)
    {
        _outputFilepath = outputFilepath;
        Chunks = new BlockingCollection<Chunk>(new ConcurrentQueue<Chunk>());
    }

    public void Run()
    {
        FileOutputWorker worker = new FileOutputWorker(Chunks, _outputFilepath);
        Thread thread = new Thread(new ThreadStart(worker.Run));
        thread.Start();
    }
}