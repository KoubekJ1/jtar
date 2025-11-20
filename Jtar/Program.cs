using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using ZstdNet;

namespace Jtar;

class Program
{
    static void Main(string[] args)
    {
        string[] filesToAdd = { "file1.txt", "file2.txt" };
        string outputTarGz = "output.tar.zlib";

        using (FileStream fsOut = new FileStream(outputTarGz, FileMode.Create))
        using (CompressionStream gz = new CompressionStream(fsOut))
        {
            foreach (var file in filesToAdd)
            {
                AddFileToTarStream(file, gz);
            }

            // Write two 512-byte blocks of zeros to mark the end of the tar archive
            gz.Write(new byte[1024], 0, 1024);
        }
    }

    static void AddFileToTarStream(string filePath, Stream outStream)
    {
        byte[] header = new byte[512];

        string fileName = Path.GetFileName(filePath);
        long fileSize = new FileInfo(filePath).Length;

        // Name (100 bytes)
        Encoding.ASCII.GetBytes(fileName).CopyTo(header, 0);

        // File mode (8 bytes)
        Encoding.ASCII.GetBytes("0000777").CopyTo(header, 100);

        // Owner numeric ID (8 bytes)
        Encoding.ASCII.GetBytes("0000000").CopyTo(header, 108);

        // Group numeric ID (8 bytes)
        Encoding.ASCII.GetBytes("0000000").CopyTo(header, 116);

        // File size in octal (12 bytes)
        string sizeOctal = Convert.ToString(fileSize, 8).PadLeft(11, '0');
        Encoding.ASCII.GetBytes(sizeOctal).CopyTo(header, 124);
        header[135] = 0; // Null terminator

        // Last modification time in octal (12 bytes)
        long mTime = new FileInfo(filePath).LastWriteTimeUtc.ToUnixTimeSeconds();
        string mTimeOctal = Convert.ToString(mTime, 8).PadLeft(11, '0');
        Encoding.ASCII.GetBytes(mTimeOctal).CopyTo(header, 136);
        header[147] = 0;

        // Checksum (will calculate after filling the rest)
        for (int i = 148; i < 156; i++) header[i] = 0x20; // spaces for checksum

        header[156] = (byte)'0'; // Type flag: normal file

        // Calculate checksum
        int checksum = 0;
        foreach (byte b in header) checksum += b;
        string checksumOctal = Convert.ToString(checksum, 8).PadLeft(6, '0');
        Encoding.ASCII.GetBytes(checksumOctal).CopyTo(header, 148);
        header[154] = 0; // Null
        header[155] = (byte)' ';

        // Write header
        //outStream.Write(header, 0, 512);

        // Write file data
        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            // Read the file bytes
            byte[] fileBytes = new byte[fileSize];
            int read = 0;
            while (read < fileBytes.Length)
            {
                int r = fs.Read(fileBytes, read, fileBytes.Length - read);
                if (r == 0) break;
                read += r;
            }

            // Combine header + file content into a single in-memory stream
            using (var combined = new MemoryStream())
            {
                combined.Write(header, 0, header.Length);
                combined.Write(fileBytes, 0, fileBytes.Length);
                combined.Position = 0;
                long remainder = combined.Length % 512;
                if (remainder != 0)
                {
                    int padding = (int)(512 - remainder);
                    outStream.Write(new byte[padding], 0, padding);
                }

                // Compress the combined stream (header + file data) using ZstdNet
                MemoryStream stream = new MemoryStream();
                using (var compressor = new CompressionStream(stream))
                {
                    //byte[] compressed = compressor.Write(combined, 0, (int)combined.Length);
                    combined.CopyTo(compressor);

                    // Write compressed block to the output stream
                    //outStream.Write(compressed, 0, compressed.Length);
                    compressor.CopyTo(outStream);

                    // Pad the compressed block to a 512-byte boundary

                }
                stream.Close();
            }
        }
    }
}

static class DateTimeExtensions
{
    public static long ToUnixTimeSeconds(this DateTime dt)
    {
        return (long)(dt - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }
}