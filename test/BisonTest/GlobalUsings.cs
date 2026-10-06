global using Xunit;

// Disabled tests running in parallel.
// Each test builds its own isolated SQLite database file, so they are
// independent; we keep parallelization off to match the rest of the suite
// and to keep output deterministic (tests share Console.Out).
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
