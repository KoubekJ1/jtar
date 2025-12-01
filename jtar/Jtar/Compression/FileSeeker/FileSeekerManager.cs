using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Jtar.Compression.FileSeeker;

public class FileSeekerManager
{
    private readonly IEnumerable<string> _inputFiles;

    private readonly BlockingCollection<string> _pathQueue;
    private readonly BlockingCollection<string> _outputQueue;

    public FileSeekerManager(IEnumerable<string> inputFiles, BlockingCollection<string> outputQueue)
    {
        _pathQueue = new BlockingCollection<string>(new ConcurrentBag<string>(inputFiles));
        _outputQueue = outputQueue;
        _inputFiles = inputFiles;
    }

    public async Task Run()
    {
        var worker = new FileSeekerWorker(_pathQueue, _outputQueue);
        await Task.Run(worker.Run);
        _outputQueue.CompleteAdding();
    }
}