using System.Collections.Concurrent;
using Jtar.Logging;

namespace Jtar.Compression.FileSeeker;

/// <summary>
/// Worker class responsible for seeking files in given directories and adding them to the output queue.
/// </summary>
public class FileSeekerWorker
{
    private readonly BlockingCollection<string> _pathQueue;
    private readonly BlockingCollection<string> _outputQueue;

    /// <summary>
    /// Initializes a new instance of the FileSeekerWorker class.
    /// </summary>
    /// <param name="directoryQueue">Input file paths to search</param>
    /// <param name="outputQueue">Queue to output found file paths into</param>
    public FileSeekerWorker(BlockingCollection<string> directoryQueue, BlockingCollection<string> outputQueue)
    {
        _pathQueue = directoryQueue;
        _outputQueue = outputQueue;
    }

    /// <summary>
    /// Starts the file seeking process.
    /// </summary>
    public void Run()
    {
        while (!_pathQueue.IsCompleted)
        {
            if (_pathQueue.Count == 0)
            {
                _pathQueue.CompleteAdding();
                Logger.Log(LogType.Debug, "Seeking completed!.");
                break;
            }
            try
            {
                var path = _pathQueue.Take();
                if (Path.IsPathRooted(path))
                {
                    Logger.Log(LogType.Error, "Path must not be an absolute path!");
                    continue;
                }
                if (!Path.Exists(path))
                {
                    Logger.Log(LogType.Error, $"Path does not exist: {path}");
                    continue;
                }

                if (Directory.Exists(path))
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
}