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

        _ = Task.Run(async () =>
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
        }, cancellationToken);

        return channel.Reader.ReadAllAsync(cancellationToken);
    }
}