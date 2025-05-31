using DirectoryHasher.Models;

namespace DirectoryHasher.Services;

public interface IResultWriter
{
    Task InitialiseAsync(CancellationToken ct);
    Task WriteAsync(Results result, CancellationToken ct);
    Task FlushAsync(CancellationToken ct);
}