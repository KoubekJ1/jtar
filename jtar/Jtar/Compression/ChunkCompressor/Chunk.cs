namespace Jtar.Compression.ChunkCompressor;

public class Chunk
{
    public string Filepath { get; private set; }
    public int Order { get; private set; }
    public int ChunkCount { get; private set; }
    public byte[] Data { get; private set; }

    public Chunk(string filepath, int order, int chunkCount, byte[] data)
    {
        Filepath = filepath;
        Order = order;
        ChunkCount = chunkCount;
        Data = data;
    }
}