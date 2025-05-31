using System.Text;
using DirectoryHasher.Models;
using DirectoryHasher.Services;
using Xunit;
using FluentAssertions;

namespace DirectoryHasher.Test;

public class CsvResultWriterTests: IDisposable
{
    private readonly string _tempFile;
    private readonly CsvResultWriter _writer;

    public CsvResultWriterTests()
    {
        _tempFile = Path.GetTempFileName();
        var options = new Options("dummy", _tempFile, 1);
        _writer = new CsvResultWriter(options);
    }

    [Fact]
    public async Task InitialiseAsync_ShouldWriteHeaderRow()
    {
        // Act
        await _writer.InitialiseAsync(CancellationToken.None);
        await _writer.FlushAsync(CancellationToken.None);

        // Assert
        var content = await File.ReadAllTextAsync(_tempFile);
        content.Should().Be("path,sha256,bytes\n");
    }

    [Fact]
    public async Task WriteAsync_WithSimplePath_ShouldWriteUnescapedPath()
    {
        // Arrange
        var result = new Results("simple/path", "hash123", 42);

        // Act
        await _writer.InitialiseAsync(CancellationToken.None);
        await _writer.WriteAsync(result, CancellationToken.None);
        await _writer.FlushAsync(CancellationToken.None);

        // Assert
        var lines = await File.ReadAllLinesAsync(_tempFile);
        lines.Should().HaveCount(2);
        lines[1].Should().Be("simple/path,hash123,42");
    }

    [Fact]
    public async Task WriteAsync_WithPathContainingComma_ShouldEscapePath()
    {
        // Arrange
        var result = new Results("path,with,commas", "hash123", 42);

        // Act
        await _writer.InitialiseAsync(CancellationToken.None);
        await _writer.WriteAsync(result, CancellationToken.None);
        await _writer.FlushAsync(CancellationToken.None);

        // Assert
        var lines = await File.ReadAllLinesAsync(_tempFile);
        lines.Should().HaveCount(2);
        lines[1].Should().Be("\"path,with,commas\",hash123,42");
    }

    [Fact]
    public async Task WriteAsync_WithPathContainingQuotes_ShouldEscapeQuotes()
    {
        // Arrange
        const string pathWithQuotes = "path\"with\"quotes";
        var result = new Results(pathWithQuotes, "hash123", 42);

        // Act
        await _writer.InitialiseAsync(CancellationToken.None);
        await _writer.WriteAsync(result, CancellationToken.None);
        await _writer.FlushAsync(CancellationToken.None);

        // Assert
        var lines = await File.ReadAllLinesAsync(_tempFile);
        lines.Should().HaveCount(2);
        lines[1].Should().Be("\"path\"\"with\"\"quotes\",hash123,42");
    }

    [Fact]
    public async Task WriteAsync_WithMultipleResults_ShouldWriteAllResults()
    {
        // Arrange
        var results = new[]
        {
            new Results("file1", "hash1", 1),
            new Results("file2", "hash2", 2),
            new Results("file3", "hash3", 3)
        };

        // Act
        await _writer.InitialiseAsync(CancellationToken.None);
        foreach (var result in results)
        {
            await _writer.WriteAsync(result, CancellationToken.None);
        }
        await _writer.FlushAsync(CancellationToken.None);

        // Assert
        var lines = await File.ReadAllLinesAsync(_tempFile);
        lines.Should().HaveCount(4);
        lines[1].Should().Be("file1,hash1,1");
        lines[2].Should().Be("file2,hash2,2");
        lines[3].Should().Be("file3,hash3,3");
    }

    [Fact]
    public async Task FlushAsync_ShouldUseUtf8Encoding()
    {
        // Arrange
        var result = new Results("path/with/utf8/☺", "hash123", 42);

        // Act
        await _writer.InitialiseAsync(CancellationToken.None);
        await _writer.WriteAsync(result, CancellationToken.None);
        await _writer.FlushAsync(CancellationToken.None);

        // Assert
        var bytes = await File.ReadAllBytesAsync(_tempFile);
        var content = Encoding.UTF8.GetString(bytes);
        content.Should().Contain("path/with/utf8/☺");
    }

    [Fact]
    public async Task FlushAsync_ShouldOverwriteExistingFile()
    {
        // Arrange
        var result1 = new Results("file1", "hash1", 1);
        var result2 = new Results("file2", "hash2", 2);

        // Act - First write
        await _writer.InitialiseAsync(CancellationToken.None);
        await _writer.WriteAsync(result1, CancellationToken.None);
        await _writer.FlushAsync(CancellationToken.None);

        // Act - Second write
        await _writer.InitialiseAsync(CancellationToken.None);
        await _writer.WriteAsync(result2, CancellationToken.None);
        await _writer.FlushAsync(CancellationToken.None);

        // Assert
        var lines = await File.ReadAllLinesAsync(_tempFile);
        lines.Should().HaveCount(2);
        lines[1].Should().Be("file2,hash2,2");
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
        {
            File.Delete(_tempFile);
        }
    }
}