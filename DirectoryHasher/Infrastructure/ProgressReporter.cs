namespace DirectoryHasher.Infrastructure;

public sealed class ProgressReporter : IProgress<long>
{
    private long _files;
    public void Report(long value)
    {
        var total = Interlocked.Add(ref _files, value);
        Console.Write($"\rFiles processed: {total:n0}");
    }
}