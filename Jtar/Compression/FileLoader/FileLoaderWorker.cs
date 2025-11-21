using System.Collections.Concurrent;
using System.Text;
using Jtar.Logging;

namespace Jtar.Compression.FileLoader;

public class FileLoaderWorker
{
    private readonly BlockingCollection<string> _filepaths;
    public FileLoaderWorker(BlockingCollection<string> filepaths)
    {
        _filepaths = filepaths;
    }

    public void Run()
    {
        while (_filepaths.Count > 0 || !_filepaths.IsCompleted)
        {
            try
            {
                string filepath = _filepaths.Take();
                Logger.Log(LogType.Debug, $"FileLoaderWorker {Environment.CurrentManagedThreadId} received: " + filepath);
                var tarStream = new MemoryStream();
                
                
            }
            catch (InvalidOperationException)
            {
                // The collection has been marked as complete for adding and is empty.
                break;
            }
        }
    }
}