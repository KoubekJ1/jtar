using System.Text;

namespace Jtar.Compression.FileLoader;

public class FileTarFormatter
{
    public FileTarFormatter()
    {
        
    }

    public byte[] FormatTar(string path)
    {
        // Ensure path is in the correct format
        path = Path.GetRelativePath(".", path);
        path = path.Replace("\\", "/");

        var fileBytes = File.ReadAllBytes(path);
        var fileLength = fileBytes.Length;
        var remainder = fileLength % 512;
        var padding = remainder > 0 ? 512 - remainder : 0;
        byte[] data = new byte[512 + fileLength + padding];
        CreateTarHeader(path, data);
        if (fileBytes.Length > 0) fileBytes.CopyTo(data, 512);

        return data;
    }

    private void WriteOctal(byte[] header, long value, int offset, int length)
    {
        string octal = Convert.ToString(value, 8).PadLeft(length - 1, '0');
        Encoding.ASCII.GetBytes(octal).CopyTo(header, offset);
    }

    private void CreateTarHeader(string path, byte[] data)
    {
        if (data.Length < 512)
            throw new ArgumentException("Data array must be at least 512 bytes long.");

        //string name = Path.GetFileName(path);
        string name = path;
        var fileInfo = new FileInfo(path);
        long size = fileInfo.Length;
        //long mtime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long mtime = fileInfo.LastWriteTime.ToFileTime();

        Encoding.ASCII.GetBytes(name).CopyTo(data, 0); // file name
        WriteOctal(data, Convert.ToInt32("644", 8), 100, 8);     // mode
        WriteOctal(data, 0, 108, 8);         // uid
        WriteOctal(data, 0, 116, 8);         // gid
        WriteOctal(data, size, 124, 12);     // size
        WriteOctal(data, mtime, 136, 12);    // mtime

        // type flag: '0' = file
        data[156] = (byte)'0';

        // magic "ustar"
        Encoding.ASCII.GetBytes("ustar").CopyTo(data, 257);

        // checksum field: fill with spaces first
        for (int i = 148; i < 156; i++)
            data[i] = 0x20;

        // compute checksum
        int chk = 0;
        for (int i = 0; i < 512; i++)
            chk += data[i];

        string chkOct = Convert.ToString(chk, 8).PadLeft(6, '0');
        Encoding.ASCII.GetBytes(chkOct).CopyTo(data, 148);
        data[154] = 0;
        data[155] = (byte)' ';
    }
}