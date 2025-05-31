namespace DirectoryHasher.Services;

public interface IFileHasher
{
    Task<(string Hash, long Bytes)> ComputeAsync(
        string filePath,
        CancellationToken cancellationToken);
}