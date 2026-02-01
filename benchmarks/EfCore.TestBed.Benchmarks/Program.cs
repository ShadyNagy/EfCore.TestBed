using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using EfCore.TestBed.Benchmarks;

var config = DefaultConfig.Instance
    .WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(50))
    .AddExporter(MarkdownExporter.GitHub)
    .AddExporter(CsvExporter.Default);

Console.WriteLine("EfCore.TestBed Benchmark Matrix");
Console.WriteLine("================================");
Console.WriteLine();
Console.WriteLine("Comparing:");
Console.WriteLine("  1. EfCore.TestBed (SQLite InMemory) - Real constraints, transactions");
Console.WriteLine("  2. SQLite Physical (File) - Real database file");
Console.WriteLine("  3. EF Core InMemory - No constraints, no real transactions");
Console.WriteLine();

if (args.Length > 0 && args[0] == "--quick")
{
    Console.WriteLine("Running quick benchmarks (subset)...");
    BenchmarkRunner.Run<DatabaseSetupBenchmarks>(config);
}
else
{
    Console.WriteLine("Select benchmark to run:");
    Console.WriteLine("  1. Database Setup");
    Console.WriteLine("  2. Insert Operations");
    Console.WriteLine("  3. Query Operations");
    Console.WriteLine("  4. Complex Queries (Joins & Aggregates)");
    Console.WriteLine("  5. Update Operations");
    Console.WriteLine("  6. Delete Operations");
    Console.WriteLine("  7. Transaction Operations");
    Console.WriteLine("  A. All Benchmarks");
    Console.WriteLine();
    Console.Write("Enter choice (1-7 or A): ");

    var choice = Console.ReadLine()?.Trim().ToUpperInvariant();

    switch (choice)
    {
        case "1":
            BenchmarkRunner.Run<DatabaseSetupBenchmarks>(config);
            break;
        case "2":
            BenchmarkRunner.Run<InsertBenchmarks>(config);
            break;
        case "3":
            BenchmarkRunner.Run<QueryBenchmarks>(config);
            break;
        case "4":
            BenchmarkRunner.Run<ComplexQueryBenchmarks>(config);
            break;
        case "5":
            BenchmarkRunner.Run<UpdateBenchmarks>(config);
            break;
        case "6":
            BenchmarkRunner.Run<DeleteBenchmarks>(config);
            break;
        case "7":
            BenchmarkRunner.Run<TransactionBenchmarks>(config);
            break;
        case "A":
            BenchmarkRunner.Run(new[]
            {
                typeof(DatabaseSetupBenchmarks),
                typeof(InsertBenchmarks),
                typeof(QueryBenchmarks),
                typeof(ComplexQueryBenchmarks),
                typeof(UpdateBenchmarks),
                typeof(DeleteBenchmarks),
                typeof(TransactionBenchmarks)
            }, config);
            break;
        default:
            Console.WriteLine("Invalid choice. Running all benchmarks...");
            BenchmarkRunner.Run(new[]
            {
                typeof(DatabaseSetupBenchmarks),
                typeof(InsertBenchmarks),
                typeof(QueryBenchmarks),
                typeof(ComplexQueryBenchmarks),
                typeof(UpdateBenchmarks),
                typeof(DeleteBenchmarks),
                typeof(TransactionBenchmarks)
            }, config);
            break;
    }
}
