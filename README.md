# directory-hasher
Directory Hasher is a .NET 8 CLI that showcases modern multithreading patterns in a real, useful tool. It

  -  Recursively scans any folder,

  -  Spools file paths through a bounded Channel<T> for back-pressure,

  -  Computes SHA-256 hashes with a configurable pool of Tasks (CPU-bound demo), and

  -  Streams results to CSV or SQLite on a dedicated reporter thread.

  -  Graceful cancellation (Ctrl-C),

  -  Progress reporting, and

  -  BenchmarkDotNet comparisons (serial vs. parallel)
    round out the sample so you can measure, not guess.