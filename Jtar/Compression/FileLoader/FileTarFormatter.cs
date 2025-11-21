using System.Text;

namespace Jtar.Compression.FileLoader;

public class FileTarFormatter
{
    public FileTarFormatter()
    {
        
    }

    public void FormatTar(string path, Stream outputStream)
    {
        
    }

    private void WriteOctal(byte[] header, long value, int offset, int length)
    {
        string octal = Convert.ToString(value, 8).PadLeft(length - 1, '0');
        Encoding.ASCII.GetBytes(octal).CopyTo(header, offset);
    }

    private void CreateTarHeader(string path, Stream outputStream)
    {
        byte[] header = new byte[512];

        string name = Path.GetFileName(path);
        long size = new FileInfo(path).Length;
        long mtime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Encoding.ASCII.GetBytes(name).CopyTo(header, 0); // file name
        WriteOctal(header, Convert.ToInt32("644", 8), 100, 8);     // mode
        WriteOctal(header, 0, 108, 8);         // uid
        WriteOctal(header, 0, 116, 8);         // gid
        WriteOctal(header, size, 124, 12);     // size
        WriteOctal(header, mtime, 136, 12);    // mtime

        // type flag: '0' = file
        header[156] = (byte)'0';

        // magic "ustar"
        Encoding.ASCII.GetBytes("ustar").CopyTo(header, 257);

        // checksum field: fill with spaces first
        for (int i = 148; i < 156; i++)
            header[i] = 0x20;

        // compute checksum
        int chk = 0;
        foreach (byte b in header)
            chk += b;

        string chkOct = Convert.ToString(chk, 8).PadLeft(6, '0');
        Encoding.ASCII.GetBytes(chkOct).CopyTo(header, 148);
        header[154] = 0;
        header[155] = (byte)' ';

        outputStream.Write(header, 0, header.Length);
    }
}