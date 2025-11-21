using System.Collections.Concurrent;

namespace Jtar.Compression.FileSeeker;

public class FileSeekerManager
{
    private readonly IEnumerable<string> _inputFiles;
    private readonly Thread _fileSeekerThread;

    private readonly BlockingCollection<string> _pathQueue;
    private readonly BlockingCollection<string> _outputQueue;

    public FileSeekerManager(IEnumerable<string> inputFiles, BlockingCollection<string> outputQueue)
    {
        _pathQueue = new BlockingCollection<string>(new ConcurrentBag<string>(inputFiles));
        _outputQueue = outputQueue;
        _inputFiles = inputFiles;

        var worker = new FileSeekerWorker(_pathQueue, _outputQueue);
        _fileSeekerThread = new Thread(new ThreadStart(worker.Run));
    }

    public void Start()
    {
        _fileSeekerThread.Start();
    }
}