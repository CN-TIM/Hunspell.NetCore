using System.IO;
using NBench;

namespace Hunspell.NetCore.Tests.Performance
{
    public class WordCheckHunspellNetCorePerfSpec : EnWordPerfBase
    {
        private Counter _wordsChecked;

        private HunspellDictionary _checker;

        [PerfSetup]
        public override void Setup(BenchmarkContext context)
        {
            base.Setup(context);

            var testAssemblyPath = Path.GetFullPath(GetType().Assembly.Location);
            var filesDirectory = Path.Combine(Path.GetDirectoryName(testAssemblyPath), "files/");
            _checker = HunspellDictionary.FromFileAsync(Path.Combine(filesDirectory, "English (American).dic")).Result;

            _wordsChecked = context.GetCounter(nameof(_wordsChecked));
        }

        [PerfBenchmark(
            Description = "How fast can Hunspell.NetCore check English (US) words?",
            NumberOfIterations = 3,
            RunMode = RunMode.Throughput,
            TestMode = TestMode.Measurement)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        [TimingMeasurement]
        [CounterMeasurement(nameof(_wordsChecked))]
        public void Benchmark(BenchmarkContext context)
        {
            foreach (var word in Words)
            {
                var result = _checker.Check(word);
                _wordsChecked.Increment();
            }
        }
    }
}
