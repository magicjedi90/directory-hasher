namespace DirectoryHasher;

public sealed record Options(
    string RootPath,
    string OutputPath,
    int MaxDegreeOfParallelism);
