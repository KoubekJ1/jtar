using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jtar.Compression.FileLoader;
using System.IO;
using System.Text;

[TestClass]
public class FileTarFormatterTests
{
    private string CreateTempFile(byte[] content, DateTime lastWrite)
    {
        string path = Path.GetTempFileName();
        File.WriteAllBytes(path, content);
        File.SetLastWriteTime(path, lastWrite);
        return path;
    }


    [TestMethod]
    public void FormatTar_ReturnsCorrectTotalLength()
    {
        var formatter = new FileTarFormatter();
        byte[] fileData = { 1, 2, 3 };
        string path = CreateTempFile(fileData, DateTime.Now);

        var tar = formatter.FormatTar(path, Path.GetTempPath());

        int expectedRemainder = fileData.Length % 512;
        int expectedPadding = expectedRemainder == 0 ? 0 : 512 - expectedRemainder;
        int expectedLength = 512 + fileData.Length + expectedPadding;

        Assert.AreEqual(expectedLength, tar.Length);
    }

    [TestMethod]
    public void FormatTar_CopiesFileContentCorrectly()
    {
        var formatter = new FileTarFormatter();
        byte[] fileData = Encoding.ASCII.GetBytes("HELLO");
        string path = CreateTempFile(fileData, DateTime.Now);

        var tar = formatter.FormatTar(path, Path.GetTempPath());

        for (int i = 0; i < fileData.Length; i++)
        {
            Assert.AreEqual(fileData[i], tar[512 + i]);
        }
    }

    [TestMethod]
    public void FormatTar_WritesCorrectSizeFieldInHeader()
    {
        var formatter = new FileTarFormatter();
        byte[] fileData = new byte[1234];
        string path = CreateTempFile(fileData, DateTime.Now);

        var tar = formatter.FormatTar(path, Path.GetTempPath());

        string sizeField = Encoding.ASCII.GetString(tar, 124, 12).TrimEnd('\0', ' ');
        string expectedOctal = Convert.ToString(1234, 8).PadLeft(11, '0');

        Assert.AreEqual(expectedOctal, sizeField);
    }

    [TestMethod]
    public void FormatTar_ComputesCorrectChecksum()
    {
        var formatter = new FileTarFormatter();

        byte[] content = new byte[] { 10, 20, 30, 40 };
        string path = CreateTempFile(content, DateTime.Now);

        var tar = formatter.FormatTar(path, Path.GetTempPath());

        // Extract checksum from TAR header
        string chkField = Encoding.ASCII.GetString(tar, 148, 6).Trim('\0', ' ');
        int chkValue = Convert.ToInt32(chkField, 8);

        // Recompute checksum manually
        int manualChk = 0;
        for (int i = 0; i < 512; i++)
        {
            byte b = tar[i];

            // Per TAR spec, checksum bytes are treated as spaces
            if (i >= 148 && i <= 155)
                b = 0x20;

            manualChk += b;
        }

        Assert.AreEqual(manualChk, chkValue);
    }

    [TestMethod]
    public void FormatTar_WritesFileNameCorrectly()
    {
        var formatter = new FileTarFormatter();

        byte[] data = { 1, 2, 3 };
        string path = CreateTempFile(data, DateTime.Now);

        var tar = formatter.FormatTar(path, Path.GetTempPath());

        string nameField = Encoding.ASCII.GetString(tar, 0, 100).Trim('\0');

        // your code writes whole path, not just filename
        Assert.AreEqual(Path.GetFileName(path), nameField, $"{path} does not match {nameField}");
    }
}