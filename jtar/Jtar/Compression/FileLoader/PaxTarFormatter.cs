using System.Formats.Tar;
using System.Text;

namespace Jtar.Compression.FileLoader;

/// <summary>
/// Formats files into PAX tar format.
/// </summary>
public class PaxTarFormatter : ITarFormatter
{
    public PaxTarFormatter()
    {
    }

    /// <summary>
    /// Formats a file into PAX tar format.
    /// </summary>
    /// <param name="path">Input filepath</param>
    /// <param name="rootDir">Root directory used to compute relative paths</param>
    public byte[] FormatTar(string path, string rootDir)
    {
        using var ms = new MemoryStream();
        using var writer = new TarWriter(ms, TarEntryFormat.Pax, leaveOpen: true);

        // Compute relative path in a normalized form
        string relative = Path.GetRelativePath(rootDir, path).Replace("\\", "/");

        var fileInfo = new FileInfo(path);

        // Create PAX entry (this automatically handles long paths, timestamps, etc.)
        var entry = new PaxTarEntry(TarEntryType.RegularFile, relative)
        {
            ModificationTime = fileInfo.LastWriteTimeUtc
        };

        // Set file size
        entry.DataStream = File.OpenRead(path);

        // Write entry
        writer.WriteEntry(entry);

        writer.Dispose(); // finalize

        var data = ms.ToArray();
        ms.Close();
        return data;
    }
}