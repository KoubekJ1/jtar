using System.Formats.Tar;
using System.Text;
using Jtar.Logging;

namespace Jtar.Compression.FileLoader;

public class FileTarFormatter
{
    public FileTarFormatter()
    {

    }

    public byte[] FormatTar(string path, string rootDir)
    {
        var fileBytes = File.ReadAllBytes(path);
        var fileLength = fileBytes.Length;
        var remainder = fileLength % 512;
        var padding = remainder > 0 ? 512 - remainder : 0;
        byte[] data = new byte[512 + fileLength + padding];
        CreateTarHeader(path, rootDir, data);
        if (fileBytes.Length > 0) fileBytes.CopyTo(data, 512);

        return data;
    }

    private void WriteOctal(byte[] header, long value, int offset, int length)
    {
        string octal = Convert.ToString(value, 8).PadLeft(length - 1, '0');
        Encoding.ASCII.GetBytes(octal).CopyTo(header, offset);
    }

    private void CreateTarHeader(string path, string rootDir, byte[] data)
    {
        if (data.Length < 512)
            throw new ArgumentException("Data array must be at least 512 bytes long.");

        // Normalize
        path = Path.GetRelativePath(rootDir, path).Replace("\\", "/");

        var filename = Path.GetFileName(path);
        var prefix = Path.GetDirectoryName(path)?.Replace("\\", "/") ?? "";

        // Byte-accurate checks
        var filenameBytes = Encoding.ASCII.GetBytes(filename);
        var prefixBytes = Encoding.ASCII.GetBytes(prefix);

        if (filenameBytes.Length > 100)
            throw new PathTooLongException("USTAR filename too long (max 100 bytes)");

        if (prefixBytes.Length > 155)
            throw new PathTooLongException("USTAR prefix too long (max 155 bytes)");

        var info = new FileInfo(Path.Combine(rootDir, path));
        long size = info.Length;
        long mtime = new DateTimeOffset(info.LastWriteTimeUtc).ToUnixTimeSeconds();

        // Zero critical fields
        Array.Clear(data, 0, 100);
        Array.Clear(data, 345, 155);

        // Write name + prefix
        filenameBytes.CopyTo(data, 0);
        prefixBytes.CopyTo(data, 345);

        // Mode, uid, gid, size, mtime
        WriteOctal(data, Convert.ToInt32("644", 8), 100, 8);
        WriteOctal(data, 0, 108, 8);
        WriteOctal(data, 0, 116, 8);
        WriteOctal(data, size, 124, 12);
        WriteOctal(data, mtime, 136, 12);

        // type flag
        data[156] = (byte)'0';

        // ustar magic + version
        Encoding.ASCII.GetBytes("ustar").CopyTo(data, 257);
        data[262] = 0; // null after magic
        Encoding.ASCII.GetBytes("00").CopyTo(data, 263);

        // checksum field = spaces
        for (int i = 148; i < 156; i++) data[i] = 0x20;

        // Compute checksum LAST
        int chk = 0;
        for (int i = 0; i < 512; i++) chk += data[i];

        var chkOct = Convert.ToString(chk, 8).PadLeft(6, '0');
        Encoding.ASCII.GetBytes(chkOct).CopyTo(data, 148);
    }
}