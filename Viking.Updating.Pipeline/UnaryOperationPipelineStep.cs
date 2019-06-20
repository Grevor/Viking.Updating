using System;

namespace Viking.Updating.Pipeline
{
    public class UnaryOperationPipelineStep<TInput, TOutput> : IPipelineStep<TOutput>
    {
        public UnaryOperationPipelineStep(string name, IPipelineStep<TInput> input, Func<TInput, TOutput> operation)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Operation = operation ?? throw new ArgumentNullException(nameof(operation));
            Signaler = Input.Signaler;
            Signaler.RegisterDependency(Input, this);
        }

        public string Name { get; }
        public IPipelineStep<TInput> Input { get; }
        public Func<TInput, TOutput> Operation { get; }
        public ISignaler Signaler { get; }

        public TOutput GetItem() => Operation(Input.GetItem());
        public void Invalidate() => Signaler.Invalidate(this);
    }
}
