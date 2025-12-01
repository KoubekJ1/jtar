using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using Jtar.Compression.FileLoader;
using Jtar.Compression.ChunkCompressor;

[TestClass]
public class FileLoaderWorkerTests
{
    [TestMethod]
    public void Run_SplitsFileIntoChunksAndPreservesData()
    {
        // Arrange – prepare the goddamn fake file
        string tempFile = Path.GetTempFileName();
        try
        {
            byte[] fileData = Encoding.ASCII.GetBytes("This is some test data for FileLoaderWorker.");
            File.WriteAllBytes(tempFile, fileData);

            var filepaths = new BlockingCollection<string>();
            var outputChunks = new BlockingCollection<Chunk>();

            filepaths.Add(tempFile);
            filepaths.CompleteAdding();

            var worker = new FileLoaderWorker(filepaths, outputChunks);

            // Act
            worker.Run();

            // Assert – check we got at least one chunk and it contains our data
            Assert.IsTrue(outputChunks.Count > 0, "No chunks were created.");
            
            // Merge all chunks back to compare
            byte[] combined = outputChunks
                .OrderBy(c => c.Order)
                .SelectMany(c => c.Data)
                .ToArray();

            // TAR prepends 512 bytes header, padding etc. + our data + padding to 1024 bytes
            Assert.AreEqual(1024, combined.Length, "Chunked data size mismatch.");

            // Check that our original file data is somewhere in the merged chunks
            CollectionAssert.AreEqual(fileData, combined.Skip(512).Take(fileData.Length).ToArray());
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [TestMethod]
    public void Run_HandlesEmptyFileGracefully()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, new byte[0]);

            var filepaths = new BlockingCollection<string>();
            var outputChunks = new BlockingCollection<Chunk>();

            filepaths.Add(tempFile);
            filepaths.CompleteAdding();

            var worker = new FileLoaderWorker(filepaths, outputChunks);
            worker.Run();

            // There should still be at least 1 chunk for the empty file because TAR adds header
            Assert.IsTrue(outputChunks.Count > 0, "Empty file produced no chunks!");
            var chunk = outputChunks.First();
            Assert.AreEqual(tempFile, chunk.Filepath);
            Assert.AreEqual(1, chunk.ChunkCount);
            Assert.AreEqual(512, chunk.Data.Length, "Empty file chunk should be just 512-byte header.");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
