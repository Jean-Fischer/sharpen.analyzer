using Xunit;

// Tests in this project mutate global static state (e.g., FirstPassSafety.Gate).
// Disable parallelization to avoid cross-test interference.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
