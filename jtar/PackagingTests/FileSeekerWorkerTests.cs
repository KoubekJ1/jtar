using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Jtar.Compression.FileSeeker;
using Jtar.Logging;

[TestClass]
public class FileSeekerWorkerTests
{
    [TestMethod]
    public void Run_EmitsFilesFromDirectory()
    {
        // Arrange – create a temp dir with some files
        //string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        string tempDir = "temp";
        Directory.CreateDirectory(tempDir);
        try
        {
            Logger.ShowDebugMessages = true;
            string file1 = Path.Combine(tempDir, "file1.txt");
            string file2 = Path.Combine(tempDir, "file2.txt");
            File.WriteAllText(file1, "hello");
            File.WriteAllText(file2, "world");

            var dirQueue = new BlockingCollection<string>();
            var outputQueue = new BlockingCollection<string>();

            dirQueue.Add(tempDir);
            //dirQueue.CompleteAdding();

            var seeker = new FileSeekerWorker(dirQueue, outputQueue);

            seeker.Run();

            var files = Directory.GetFiles(tempDir);
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }

            // Assert – outputQueue should contain the files
            var filesFound = outputQueue.ToList();
            Console.WriteLine($"Files found: {string.Join(", ", filesFound)}");
            CollectionAssert.Contains(filesFound, file1);
            CollectionAssert.Contains(filesFound, file2);
            Assert.AreEqual(2, filesFound.Count, "Expected exactly 2 files in the output queue.");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [TestMethod]
    public void Run_EmitsSingleFilePath()
    {
        // Arrange – single file, not a directory
        //string tempFile = Path.GetTempFileName();
        try
        {
            var path = "temp.txt";
            File.WriteAllText(path, "test");

            var inputQueue = new BlockingCollection<string>();
            var outputQueue = new BlockingCollection<string>();

            inputQueue.Add(path);
            //inputQueue.CompleteAdding();

            var seeker = new FileSeekerWorker(inputQueue, outputQueue);

            // Act
            seeker.Run();

            // Assert
            var output = outputQueue.ToList();
            Assert.AreEqual(1, output.Count, "Expected exactly 1 file in output queue.");
            Assert.AreEqual(path, output[0], "Output file path does not match input.");
        }
        finally
        {
            //File.Delete(tempFile);
        }
    }

    [TestMethod]
    public void Run_SkipsNonExistentPaths()
    {
        // Arrange – give it a path that doesn't exist
        string fakePath = Path.Combine(Path.GetTempPath(), "definitely_not_a_real_file.txt");

        var inputQueue = new BlockingCollection<string>();
        var outputQueue = new BlockingCollection<string>();

        inputQueue.Add(fakePath);
        inputQueue.CompleteAdding();

        var seeker = new FileSeekerWorker(inputQueue, outputQueue);

        // Act
        seeker.Run();

        // Assert – output should be empty
        Assert.AreEqual(0, outputQueue.Count, "Output queue should be empty because path does not exist.");
    }
}
