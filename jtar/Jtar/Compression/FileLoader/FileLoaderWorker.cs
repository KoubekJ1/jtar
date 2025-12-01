using System.Collections.Concurrent;
using System.Text;
using Jtar.Compression.ChunkCompressor;
using Jtar.Logging;

namespace Jtar.Compression.FileLoader;

public class FileLoaderWorker
{
    private const long MAX_CHUNK_SIZE_BYTES = 128 * 1024; // 128 KB (limit set by the zstd specification)

    private readonly BlockingCollection<string> _filepaths;
    private readonly FileTarFormatter _fileTarFormatter;
    private readonly BlockingCollection<Chunk> _outputCollection;
    public FileLoaderWorker(BlockingCollection<string> filepaths, BlockingCollection<Chunk> outputCollection)
    {
        _filepaths = filepaths;
        _fileTarFormatter = new FileTarFormatter();
        _outputCollection = outputCollection;
    }

    public void Run()
    {
        while (!_filepaths.IsCompleted)
        {
            try
            {
                string filepath = _filepaths.Take();
                Logger.Log(LogType.Debug, $"FileLoaderWorker {Environment.CurrentManagedThreadId} received: " + filepath);

                byte[] data;
                try
                {
                    data = _fileTarFormatter.FormatTar(filepath, ".");
                }
                catch (PathTooLongException)
                {
                    Logger.Log(LogType.Error, $"File path is too long: {filepath}");
                    continue;
                }
                catch (Exception e)
                {
                    Logger.Log(LogType.Error, $"Unknown error while loading: {filepath}");
                    Logger.Log(LogType.Debug, e.ToString());
                    continue;
                }

                int totalChunks = (int)Math.Ceiling((double)data.Length / MAX_CHUNK_SIZE_BYTES);
                for (int i = 0; i < totalChunks; i++)
                {
                    int chunkSize = (int)Math.Min(MAX_CHUNK_SIZE_BYTES, data.Length - (i * MAX_CHUNK_SIZE_BYTES));
                    byte[] chunkData = new byte[chunkSize];
                    Array.Copy(data, i * MAX_CHUNK_SIZE_BYTES, chunkData, 0, chunkSize);

                    var chunk = new Chunk(filepath, i, totalChunks, chunkData);

                    _outputCollection.Add(chunk);
                }
                }
                catch (InvalidOperationException)
                {
                    // The collection has been marked as complete for adding and is empty.
                    Logger.Log(LogType.Debug, $"FileLoaderWorker {Environment.CurrentManagedThreadId} interrupted and finishing");
                    break;
                }
            }
    }
    }