using ZstdNet;

namespace Jtar.Compression.Compressor;

public class ZstdCompressor : ICompressor
{
    private readonly ZstdNet.Compressor _compressor;
    private readonly ZstdNet.Decompressor _decompressor;
    public ZstdCompressor()
    {
        _compressor = new ZstdNet.Compressor(new CompressionOptions(CompressionOptions.MaxCompressionLevel));
        _decompressor = new ZstdNet.Decompressor();
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
        return _compressor.Wrap(data);
    }

    public byte[] Decompress(byte[] data)
    {
        return _decompressor.Unwrap(data);
    }
}