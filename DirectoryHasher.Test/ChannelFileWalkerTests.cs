using DirectoryHasher.Services;
using Xunit;
using Assert = Xunit.Assert;

namespace DirectoryHasher.Test;

public class ChannelFileWalkerTests
{
    private readonly ChannelFileWalker _sut = new();
    
    [Fact]
    public async Task EnumerateFilesAsync_WithValidPath_ReturnsAllFiles()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempPath);
        var subDir = Path.Combine(tempPath, "subdir");
        Directory.CreateDirectory(subDir);
        
        var expectedFiles = new[]
        {
            Path.Combine(tempPath, "file1.txt"),
            Path.Combine(tempPath, "file2.txt"),
            Path.Combine(subDir, "file3.txt")
        };
        
        foreach (var file in expectedFiles)
        {
            await File.WriteAllTextAsync(file, "test");
        }

        try
        {
            // Act
            var result = new List<string>();
            await foreach (var file in _sut.EnumerateFilesAsync(tempPath, CancellationToken.None))
            {
                result.Add(file);
            }

            // Assert
            Assert.Equal(expectedFiles.Length, result.Count);
            foreach (var expectedFile in expectedFiles)
            {
                Assert.Contains(expectedFile, result);
            }
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempPath, recursive: true);
        }
    }

    [Fact]
    public async Task EnumerateFilesAsync_WithCancellation_StopsEnumeration()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempPath);
        await File.WriteAllTextAsync(Path.Combine(tempPath, "file1.txt"), "test");
        
        using var cts = new CancellationTokenSource();

        try
        {
            // Act & Assert
            var enumeration = _sut.EnumerateFilesAsync(tempPath, cts.Token);
            await cts.CancelAsync();
            
            await Assert.ThrowsAsync<TaskCanceledException>(async () => 
            {
                await foreach (var _ in enumeration.ConfigureAwait(false))
                {
                    break; // We only need to attempt the first iteration
                }
            });
        }
        finally
        {
            Directory.Delete(tempPath, recursive: true);
        }
    }
}