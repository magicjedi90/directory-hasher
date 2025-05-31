namespace DirectoryHasher.Models;

public sealed record Results(
    string FilePath,
    string Sha256,
    long   Bytes);