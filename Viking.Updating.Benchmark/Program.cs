using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;

namespace Viking.Updating.Benchmark
{
    [RPlotExporter]
    public class Benchmarks
    {
        [Params(1,2,4,8,16,32,64,128,256,512,1024,2048, 10000)]
        public int Updaters { get; set; }

        [Benchmark]
        public int RunStuff()
        {
            var graph = new UpdateGraph<int>();
            for (var i = 0; i < Updaters; ++i)
                graph.AddUpdate(new[] { i }, DoNothing, new[] { i + 1 });

            graph.Trigger(0);
            return Updaters;
        }

        private static void DoNothing() { }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
