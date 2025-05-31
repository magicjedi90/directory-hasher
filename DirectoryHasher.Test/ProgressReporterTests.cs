using DirectoryHasher.Infrastructure;
using Xunit;
using Assert = Xunit.Assert;

namespace DirectoryHasher.Test
{
    public class ProgressReporterTests
    {
        [Fact]
        public void Report_ShouldAccumulateValues()
        {
            // Arrange
            var reporter = new ProgressReporter();
            
            // Act
            reporter.Report(1);
            reporter.Report(2);
            reporter.Report(3);
            
            // Use reflection to access the private field for verification
            var fieldInfo = typeof(ProgressReporter)
                .GetField("_files", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var finalValue = (long)fieldInfo!.GetValue(reporter)!;
            
            // Assert
            Assert.Equal(6L, finalValue);
        }

        [Fact]
        public async Task Report_WithConcurrentAccess_ShouldAccumulateCorrectly()
        {
            // Arrange
            var reporter = new ProgressReporter();
            const int iterations = 1000;
            var tasks = Enumerable.Range(0, 3)
                .Select(_ => Task.Run(() =>
                {
                    foreach (var _ in Enumerable.Range(0, iterations))
                    {
                        reporter.Report(1);
                    }
                }))
                .ToArray();

            // Act
            await Task.WhenAll(tasks);

            // Use reflection to access the private field for verification
            var fieldInfo = typeof(ProgressReporter)
                .GetField("_files", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var finalValue = (long)fieldInfo!.GetValue(reporter)!;

            // Assert
            var expectedTotal = (long)iterations * tasks.Length;
            Assert.Equal(expectedTotal, finalValue);
        }
    }
}