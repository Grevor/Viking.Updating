using System;
using Viking.Updating.Pipeline;

namespace Viking.Updating.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var signaler = new UpdateSignaler();
            var provider = new AssignableValuePipelineStep<int>("provider", 1, signaler);

            var doubleIt = new UnaryOperationPipelineStep<int, int>("double it!", provider, a => a * 2);
            var addition = new BinaryOperationPipelineStep<int, int, int>("addition", provider, doubleIt, (a, b) => a + b);

            var pauser = new PausePipelineStep<int>("pauser", addition);
            var cache = new CachePipelineStep<int>(pauser);

            var writeIt = new ActionPipelineStep<int>("write it to console", cache, WriteToConsole);
            var error = new ActionPipelineStep<int>("error!", cache, Error);

            provider.Invalidate();
            provider.Set(10);

            pauser.Pause();
            provider.Set(2);
            provider.Set(3);

            pauser.Unpause(true);
        }

        private static void WriteToConsole(int value)
        {
            Console.WriteLine($"Value changed to {value}");
        }
        private static void Error(int value)
        {
            throw new Exception();
        }
    }
}
