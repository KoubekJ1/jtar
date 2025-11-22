namespace Jtar.Compression.ChunkCompresser;

public class Chunk
{
    public string Filepath { get; private set; }
    public int Order { get; private set; }
    public byte[] Data { get; private set; }

    public Chunk(string filepath, int order, byte[] data)
    {
        Filepath = filepath;
        Order = order;
        Data = data;
    }
}