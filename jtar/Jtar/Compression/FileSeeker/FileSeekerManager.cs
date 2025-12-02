using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Jtar.Compression.FileSeeker;

/// <summary>
/// Manages the file seeking process.
/// Instances of this class manage workers that search for files in the provided input paths
/// and enqueue the found file paths into an output queue
/// </summary>
public class FileSeekerManager
{
    private readonly IEnumerable<string> _inputFiles;

    private readonly BlockingCollection<string> _pathQueue;
    private readonly BlockingCollection<string> _outputQueue;

    /// <summary>
    /// Constructs a new FileSeekerManager.
    /// </summary>
    /// <param name="inputFiles">Files to be searched</param>
    /// <param name="outputQueue">Queue to output found file paths into</param>
    public FileSeekerManager(IEnumerable<string> inputFiles, BlockingCollection<string> outputQueue)
    {
        _pathQueue = new BlockingCollection<string>(new ConcurrentBag<string>(inputFiles));
        _outputQueue = outputQueue;
        _inputFiles = inputFiles;
    }

    /// <summary>
    /// Starts the file seeking process.
    /// </summary>
    public async Task Run()
    {
        var worker = new FileSeekerWorker(_pathQueue, _outputQueue);
        await Task.Run(worker.Run);
        _outputQueue.CompleteAdding();
    }
}