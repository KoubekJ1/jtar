namespace Jtar.Compression.Compressor;

public class ZstdCompressor : ICompressor
{
    private readonly ZstdSharp.Compressor _compressor;
    private readonly ZstdSharp.Decompressor _decompressor;
    public ZstdCompressor()
    {
        _compressor = new ZstdSharp.Compressor(ZstdSharp.Compressor.MaxCompressionLevel);
        _decompressor = new ZstdSharp.Decompressor();
    }

    ~ZstdCompressor()
    {
        _compressor.Dispose();
        _decompressor.Dispose();
    }

    public object Clone()
    {
        return new ZstdCompressor();
    }

    public byte[] Compress(byte[] data)
    {
        return _compressor.Wrap(data).ToArray();
    }

    public byte[] Decompress(byte[] data)
    {
        return _decompressor.Unwrap(data).ToArray();
    }
}