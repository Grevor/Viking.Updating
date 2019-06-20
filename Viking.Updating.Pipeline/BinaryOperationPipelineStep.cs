using System;

namespace Viking.Updating.Pipeline
{
    public class BinaryOperationPipelineStep<TInput1, TInput2, TOutput> : IPipelineStep<TOutput>
    {
        public BinaryOperationPipelineStep(string name, IPipelineStep<TInput1> input1, IPipelineStep<TInput2> input2, Func<TInput1, TInput2, TOutput> operation)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Input1 = input1 ?? throw new ArgumentNullException(nameof(input1));
            Input2 = input2 ?? throw new ArgumentNullException(nameof(input2));
            Operation = operation ?? throw new ArgumentNullException(nameof(operation));
            Signaler = Input1.Signaler;
            Signaler.RegisterMultiple(this, Input1, Input2);
        }

        public string Name { get; }
        public IPipelineStep<TInput1> Input1 { get; }
        public IPipelineStep<TInput2> Input2 { get; }
        public Func<TInput1, TInput2, TOutput> Operation { get; }
        public ISignaler Signaler { get; }

        public TOutput GetItem() => Operation(Input1.GetItem(), Input2.GetItem());

        public void Invalidate() => Signaler.Invalidate(this);
    }
}
