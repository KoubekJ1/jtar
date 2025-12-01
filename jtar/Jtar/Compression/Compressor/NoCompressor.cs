namespace Jtar.Compression.Compressor;

public class NoCompressor : ICompressor
{
    public object Clone()
    {
        return new NoCompressor();
    }

    public byte[] Compress(byte[] data)
    {
        return data;
    }

    public byte[] Decompress(byte[] data)
    {
        return data;
    }
}