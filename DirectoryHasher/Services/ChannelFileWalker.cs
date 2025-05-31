using System.Threading.Channels;

namespace DirectoryHasher.Services;

public sealed class ChannelFileWalker : IFileWalker
{
    private const int ChannelCapacity = 8_192;

    public IAsyncEnumerable<string> EnumerateFilesAsync(
        string rootPath,
        CancellationToken cancellationToken)
    {
        var channel = Channel.CreateBounded<string>(
            new BoundedChannelOptions(ChannelCapacity)
            {
                SingleWriter = true,
                FullMode     = BoundedChannelFullMode.Wait
            });

        _ = Task.Run(() => WriteFilesToChannelAsync(rootPath, channel, cancellationToken), cancellationToken);

        return channel.Reader.ReadAllAsync(cancellationToken);
    }

    private static async Task WriteFilesToChannelAsync(string rootPath,
        Channel<string> channel, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var file in Directory.EnumerateFiles(
                         rootPath, "*", SearchOption.AllDirectories))
            {
                await channel.Writer.WriteAsync(file, cancellationToken);
            }
        }
        finally
        {
            channel.Writer.TryComplete();
        }
    }
}