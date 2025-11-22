using System.Collections.Concurrent;
using System.Text;
using Jtar.Compression.ChunkCompresser;
using Jtar.Logging;

namespace Jtar.Compression.FileLoader;

public class FileLoaderWorker
{
    private const long MAX_CHUNK_SIZE_BYTES = 128 * 1024; // 128 KB

    private readonly BlockingCollection<string> _filepaths;
    private readonly FileTarFormatter _fileTarFormatter;
    public FileLoaderWorker(BlockingCollection<string> filepaths)
    {
        _filepaths = filepaths;
        _fileTarFormatter = new FileTarFormatter();
    }

    public void Run()
    {
        while (_filepaths.Count > 0 || !_filepaths.IsCompleted)
        {
            try
            {
                string filepath = _filepaths.Take();
                Logger.Log(LogType.Debug, $"FileLoaderWorker {Environment.CurrentManagedThreadId} received: " + filepath);
                
                var data = _fileTarFormatter.FormatTar(filepath);

                int totalChunks = (int)Math.Ceiling((double)data.Length / MAX_CHUNK_SIZE_BYTES);
                for (int i = 0; i < totalChunks; i++)
                {
                    int chunkSize = (int)Math.Min(MAX_CHUNK_SIZE_BYTES, data.Length - (i * MAX_CHUNK_SIZE_BYTES));
                    byte[] chunkData = new byte[chunkSize];
                    Array.Copy(data, i * MAX_CHUNK_SIZE_BYTES, chunkData, 0, chunkSize);

                    var chunk = new Chunk(filepath, i, chunkData);
                }
            }
            catch (InvalidOperationException)
            {
                // The collection has been marked as complete for adding and is empty.
                break;
            }
        }
    }
}