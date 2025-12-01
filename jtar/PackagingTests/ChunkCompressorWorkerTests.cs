using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Linq;
using Jtar.Compression.ChunkCompressor;
using Jtar.Compression.Compressor;

[TestClass]
public class ChunkCompressorWorkerTests
{
    [TestMethod]
    public void Run_CompressesChunksCorrectly()
    {
        // Arrange
        var inputChunks = new BlockingCollection<Chunk>();
        var outputChunks = new BlockingCollection<Chunk>();

        // simple chunk data
        var originalData = new byte[] { 1, 2, 3, 4, 5 };
        var chunk = new Chunk("file.txt", 0, 1, originalData);
        inputChunks.Add(chunk);
        inputChunks.CompleteAdding();

        var compressor = new NoCompressor(); // your no-op compressor
        var worker = new ChunkCompressorWorker(inputChunks, compressor, outputChunks);

        // Act
        worker.Run();

        // Assert
        Assert.AreEqual(1, outputChunks.Count);
        var result = outputChunks.First();
        Assert.AreEqual(chunk.Filepath, result.Filepath);
        Assert.AreEqual(chunk.Order, result.Order);
        Assert.AreEqual(chunk.ChunkCount, result.ChunkCount);
        CollectionAssert.AreEqual(originalData, result.Data);
    }

    [TestMethod]
    public void Run_DoesNotThrow_WhenInputCollectionIsEmpty()
    {
        // Arrange
        var inputChunks = new BlockingCollection<Chunk>();
        inputChunks.CompleteAdding(); // empty collection
        var outputChunks = new BlockingCollection<Chunk>();
        var worker = new ChunkCompressorWorker(inputChunks, new NoCompressor(), outputChunks);

        // Act & Assert
        worker.Run(); // should not throw
        Assert.AreEqual(0, outputChunks.Count);
    }
}
