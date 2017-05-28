﻿using NBench;

namespace Hunspell.NetCore.Tests.Performance
{
    public class FileLoadHunspellNetCorePerfSpec : FileLoadPerfBase
    {
        protected Counter FilePairsLoaded;

        [PerfSetup]
        public override void Setup(BenchmarkContext context)
        {
            base.Setup(context);

            FilePairsLoaded = context.GetCounter(nameof(FilePairsLoaded));
        }

        [PerfBenchmark(
            Description = "How fast can Hunspell.NetCore load files?",
            NumberOfIterations = 2,
            RunMode = RunMode.Throughput,
            TestMode = TestMode.Measurement)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        [TimingMeasurement]
        [CounterMeasurement(nameof(FilePairsLoaded))]
        public void Benchmark(BenchmarkContext context)
        {
            foreach(var filePair in TestFiles)
            {
                var checker = HunspellDictionary.FromFileAsync(filePair.DictionaryFilePath, filePair.AffixFilePath).Result;
                checker.Check(TestWord);
                FilePairsLoaded.Increment();
            }
        }
    }
}
