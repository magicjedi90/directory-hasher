using System.Security.Cryptography;
using DirectoryHasher.Services;
using FluentAssertions;
using Xunit;
using Assert = Xunit.Assert;

namespace DirectoryHasher.Test;

public class Sha256FileHasherTests : IDisposable
{
    private readonly string _testFilePath = Path.GetTempFileName();
    private readonly Sha256FileHasher _hasher = new();

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    [Fact]
    public async Task ComputeAsync_WithKnownContent_ReturnsCorrectHash()
    {
        // Arrange
        var content = "Hello, World!"u8.ToArray();
        await File.WriteAllBytesAsync(_testFilePath, content);
        var expectedHash = Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();
        var expectedLength = content.Length;

        // Act
        var result = await _hasher.ComputeAsync(_testFilePath, CancellationToken.None);

        // Assert
        result.Hash.Should().Be(expectedHash);
        result.Bytes.Should().Be(expectedLength);
    }

    [Fact]
    public async Task ComputeAsync_WithEmptyFile_ReturnsValidHashAndZeroLength()
    {
        // Arrange
        await File.WriteAllBytesAsync(_testFilePath, []);
        const string expectedHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

        // Act
        var result = await _hasher.ComputeAsync(_testFilePath, CancellationToken.None);

        // Assert
        result.Hash.Should().Be(expectedHash);
        result.Bytes.Should().Be(0);
    }

    [Fact]
    public async Task ComputeAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _hasher.ComputeAsync(nonExistentPath, CancellationToken.None));
    }

    [Fact]
    public async Task ComputeAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var content = new byte[1024 * 1024]; // 1MB file
        await File.WriteAllBytesAsync(_testFilePath, content);
        
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _hasher.ComputeAsync(_testFilePath, cts.Token));
    }

    [Fact]
    public async Task ComputeAsync_ReturnsHashInCorrectFormat()
    {
        // Arrange
        await File.WriteAllTextAsync(_testFilePath, "Test content");

        // Act
        var result = await _hasher.ComputeAsync(_testFilePath, CancellationToken.None);

        // Assert
        result.Hash.Should().MatchRegex("^[0-9a-f]{64}$");
    }
}