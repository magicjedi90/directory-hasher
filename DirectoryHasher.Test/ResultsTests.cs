using DirectoryHasher.Models;
using Xunit;
using Assert = Xunit.Assert;

namespace DirectoryHasher.Test;

public class ResultsTests
{
    private const string FilePath = "/path/to/file";
    private const string Sha256 = "abc123";
    private const long Bytes = 1234L;
    
    [Fact]
    public void Constructor_ShouldCreateInstance_WithCorrectProperties()
    {
        // Act
        var results = new Results(FilePath, Sha256, Bytes);

        // Assert
        Assert.Equal(FilePath, results.FilePath);
        Assert.Equal(Sha256, results.Sha256);
        Assert.Equal(Bytes, results.Bytes);
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenPropertiesAreIdentical()
    {
        // Arrange
        var results1 = new Results(FilePath, Sha256, Bytes);
        var results2 = new Results(FilePath, Sha256, Bytes);

        // Act & Assert
        Assert.Equal(results1, results2);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenPropertiesDiffer()
    {
        // Arrange
        var results1 = new Results(FilePath, Sha256, Bytes);
        var results2 = new Results("/different/path", "abc123", 1234L);

        // Act & Assert
        Assert.NotEqual(results1, results2);
    }

    [Fact]
    public void WithFilePath_ShouldCreateNewInstance_WithUpdatedFilePath()
    {
        // Arrange
        var original = new Results(FilePath, Sha256, Bytes);
        const string newPath = "/new/path";

        // Act
        var modified = original with { FilePath = newPath };

        // Assert
        Assert.Equal(newPath, modified.FilePath);
        Assert.Equal(original.Sha256, modified.Sha256);
        Assert.Equal(original.Bytes, modified.Bytes);
        Assert.NotEqual(original, modified);
    }
}