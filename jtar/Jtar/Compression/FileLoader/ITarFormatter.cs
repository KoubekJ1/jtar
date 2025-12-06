namespace Jtar.Compression.FileLoader;

public interface ITarFormatter
{
    byte[] FormatTar(string path, string rootDir);
}