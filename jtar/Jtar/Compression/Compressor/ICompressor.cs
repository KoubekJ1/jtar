namespace Jtar.Compression.Compressor;

public interface ICompressor : ICloneable
{
    byte[] Compress(byte[] data);
    byte[] Decompress(byte[] data);
}