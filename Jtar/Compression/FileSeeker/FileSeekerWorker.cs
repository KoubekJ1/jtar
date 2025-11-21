using System.Collections.Concurrent;
using Jtar.Logging;

namespace Jtar.Compression.FileSeeker;

public class FileSeekerWorker
{
    private readonly BlockingCollection<string> _pathQueue;
    private readonly BlockingCollection<string> _outputQueue;

    public FileSeekerWorker(BlockingCollection<string> directoryQueue, BlockingCollection<string> outputQueue)
    {
        _pathQueue = directoryQueue;
        _outputQueue = outputQueue;
    }

    public void Run()
    {
        while (!_pathQueue.IsCompleted)
        {
            try
            {
                var path = _pathQueue.Take();
                if (!Path.Exists(path))
                {
                    Logger.Log(LogType.Error, $"Path does not exist: {path}");
                    continue;
                }

                if (IsDirectory(path))
                {
                    var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        _pathQueue.Add(file);
                    }
                }
                else
                {
                    _outputQueue.Add(path);
                }
            }
            catch (InvalidOperationException)
            {
                // The collection has been marked as complete for adding.
                break;
            }
        }
    }

    private bool IsDirectory(string path)
    {
        return Directory.Exists(path);
    }
}