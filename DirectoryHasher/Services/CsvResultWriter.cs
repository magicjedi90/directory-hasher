using DirectoryHasher.Models;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace DirectoryHasher.Services;

public sealed class CsvResultWriter(Options opts) : IResultWriter
{
    private readonly ConcurrentQueue<string> _buffer = new();

    public Task InitialiseAsync(CancellationToken ct)
    {
        // Header row
        _buffer.Enqueue("path,sha256,bytes");
        return Task.CompletedTask;
    }

    public Task WriteAsync(Results result, CancellationToken ct)
    {
        var line = string.Create(CultureInfo.InvariantCulture,
            $"{Escape(result.FilePath)},{result.Sha256},{result.Bytes}");
        _buffer.Enqueue(line);
        return Task.CompletedTask;
    }

    public async Task FlushAsync(CancellationToken ct)
    {
        await using var writer = new StreamWriter(
            opts.OutputPath, append: false, Encoding.UTF8);

        while (_buffer.TryDequeue(out var line))
            await writer.WriteLineAsync(line);
    }

    private static string Escape(string path)
    {
        var needsQuotes = path.Contains(',') || path.Contains('"');
        var escaped = path.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{escaped}\"" : path;
    }

}