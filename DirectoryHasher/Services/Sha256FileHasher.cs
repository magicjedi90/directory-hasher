using System.Security.Cryptography;
using System.Text;

namespace DirectoryHasher.Services;

public sealed class Sha256FileHasher : IFileHasher
{
    public async Task<(string Hash, long Bytes)> ComputeAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        await using var fileStream = File.OpenRead(filePath);
        using var sha = SHA256.Create();

        var hash = await sha.ComputeHashAsync(fileStream, cancellationToken);
        var hex  = ConvertToHex(hash);
        return (hex, fileStream.Length);
    }

    private static string ConvertToHex(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}