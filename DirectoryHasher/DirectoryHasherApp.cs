using DirectoryHasher.Services;
using DirectoryHasher.Infrastructure;
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;
using DirectoryHasher.Models;

namespace DirectoryHasher;

public sealed class DirectoryHasherApp(
    IFileWalker walker,
    IFileHasher hasher,
    IResultWriter writer,
    ProgressReporter progress,
    Options opts,
    IHostApplicationLifetime hostLifetime)
    : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await writer.InitialiseAsync(cancellationToken);

        var resultsChannel = Channel.CreateUnbounded<Results>();

        // Producer – enumerates paths
        var producer = Task.Run(async () =>
        {
            await foreach (var path in walker.EnumerateFilesAsync(opts.RootPath, cancellationToken))
            {
                var (hash, bytes) = await hasher.ComputeAsync(path, cancellationToken);
                await resultsChannel.Writer.WriteAsync(
                    new Results(path, hash, bytes), cancellationToken);

                progress.Report(1);
            }
            resultsChannel.Writer.TryComplete();
        }, cancellationToken);

        // Consumer – writes results
        var consumer = Task.Run(async () =>
        {
            await foreach (var result in resultsChannel.Reader.ReadAllAsync(cancellationToken))
            {
                await writer.WriteAsync(result, cancellationToken);
            }
        }, cancellationToken);

        await Task.WhenAll(producer, consumer);
        await writer.FlushAsync(cancellationToken);

        Console.WriteLine("\nDone.");
        hostLifetime.StopApplication();
    }
}