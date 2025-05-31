// Program.cs (top-level statements – .NET 8)

using System.CommandLine;
using DirectoryHasher;
using DirectoryHasher.Infrastructure;
using DirectoryHasher.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var rootCommand = new RootCommand("Directory Hasher – parallel SHA-256 tool");

var dirArg = new Argument<string>(
        name: "path",
        description: "Root directory to scan")
    { Arity = ArgumentArity.ExactlyOne };

var outFileOption = new Option<string>(
    name: "--out",
    description: "Output file (CSV or .sqlite)",
    getDefaultValue: () => "hashes.csv");

var maxThreadsOption = new Option<int>(
    name: "--threads",
    description: "Max worker tasks (default = logical CPU count)",
    getDefaultValue: () => Environment.ProcessorCount);

rootCommand.Add(dirArg);
rootCommand.Add(outFileOption);
rootCommand.Add(maxThreadsOption);

rootCommand.SetHandler(async (ctx) =>
{
    var path = ctx.ParseResult.GetValueForArgument(dirArg);
    var outFile = ctx.ParseResult.GetValueForOption(outFileOption);
    var threads = ctx.ParseResult.GetValueForOption(maxThreadsOption);

    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

    var host = Host.CreateDefaultBuilder()
        .ConfigureServices(services =>
        {
            if (outFile != null) services.AddSingleton(new Options(path, outFile, threads));
            services.AddSingleton<IFileWalker, ChannelFileWalker>();
            services.AddSingleton<IFileHasher, Sha256FileHasher>();
            services.AddSingleton<IResultWriter, CsvResultWriter>();
            services.AddSingleton<ProgressReporter>();
            services.AddHostedService<DirectoryHasherApp>();
        })
        .Build();

    await host.RunAsync(cts.Token);
});

return await rootCommand.InvokeAsync(args);