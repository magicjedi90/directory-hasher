namespace DirectoryHasher.Services;

public interface IFileWalker
{
    IAsyncEnumerable<string> EnumerateFilesAsync(
        string rootPath,
        CancellationToken cancellationToken);
}