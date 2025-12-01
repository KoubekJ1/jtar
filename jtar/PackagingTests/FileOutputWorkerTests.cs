using System.Collections.Concurrent;
using System.Reflection;
using Jtar.Compression.ChunkCompressor;
using Jtar.Compression.Compressor;
using Jtar.Compression.FileOutput;

namespace PackagingTests;

[TestClass]
public class FileOutputWorkerTests
{
    [TestMethod]
    public void TestWritesChunksInOrder()
    {
        // Arrange
        var bc = new BlockingCollection<Chunk>();
        var compressor = new NoCompressor();
        var ms = new MemoryStream();

        var worker = new FileOutputWorker(bc, compressor, ms);

        // push chunks out of order
        bc.Add(new Chunk("fileA", 1, 2, new byte[] { 2 }));
        bc.Add(new Chunk("fileA", 0, 2, new byte[] { 1 }));
        bc.CompleteAdding();

        // Act
        worker.Run();
        ms.Position = 0;
        var output = ms.ToArray();

        // Assert
        var expected = new byte[] { 1, 2 }.Concat(new byte[1024]).ToArray();
        Assert.IsTrue(expected.SequenceEqual(output.Take(1026)), $"Expected [{string.Join(",", expected)}], got [{string.Join(",", output)}]");
        Assert.IsTrue(output.Length > 2); // includes compressed end block
    }

    [TestMethod]
    public void TestThrowsOnChunkCountMismatch()
    {
        // Arrange
        var bc = new BlockingCollection<Chunk>();
        var compressor = new NoCompressor();
        var ms = new MemoryStream();
        var worker = new FileOutputWorker(bc, compressor, ms);

        bc.Add(new Chunk("fileA", 0, 2, new byte[] { 1 }));
        bc.Add(new Chunk("fileA", 1, 3, new byte[] { 2 })); // wrong total count
        bc.CompleteAdding();

        // Act & Assert
        Assert.ThrowsException<InvalidDataException>(() => worker.Run());
    }

    [TestMethod]
    public void TestHandlesMultipleFiles()
    {
        // Arrange
        var bc = new BlockingCollection<Chunk>();
        var compressor = new NoCompressor();
        var ms = new MemoryStream();
        var worker = new FileOutputWorker(bc, compressor, ms);

        bc.Add(new Chunk("B", 1, 2, new byte[] { 21 }));
        bc.Add(new Chunk("A", 0, 1, new byte[] { 10 }));
        bc.Add(new Chunk("B", 0, 2, new byte[] { 20 }));
        bc.CompleteAdding();

        // Act
        worker.Run();
        var output = ms.ToArray();

        // Assert
        Assert.IsTrue(output.Contains((byte)10));
        Assert.IsTrue(output.Contains((byte)20));
        Assert.IsTrue(output.Contains((byte)21));
    }

    [TestMethod]
    public void TestAppendsCompressedEndBlock()
    {
        // Arrange
        var bc = new BlockingCollection<Chunk>();
        var compressor = new NoCompressor();
        var ms = new MemoryStream();
        var worker = new FileOutputWorker(bc, compressor, ms);

        bc.CompleteAdding();

        // Act
        worker.Run();
        var data = ms.ToArray();

        // Assert
        Assert.IsTrue(data.Length > 0);
        Assert.AreEqual(1024, data.Length); // since FakeCompressor returns input unchanged
    }

}
