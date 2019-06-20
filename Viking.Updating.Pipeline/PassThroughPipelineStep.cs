using System;

namespace Viking.Updating.Pipeline
{
    public class PassThroughPipelineStep<TOutput> : IPipelineStep<TOutput>
    {
        public PassThroughPipelineStep(string name, IPipelineStep<TOutput> input, bool invalidateOnInvalidate, bool dependOnInput)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            InvalidateOnInvalidate = invalidateOnInvalidate;
            if (dependOnInput)
                Signaler.RegisterDependency(Input, this);
        }

        public string Name { get; }
        public IPipelineStep<TOutput> Input { get; }
        public bool InvalidateOnInvalidate { get; }

        public ISignaler Signaler => Input.Signaler;

        public TOutput GetItem() => Input.GetItem();

        public void Invalidate()
        {
            if (InvalidateOnInvalidate)
                Signaler.Invalidate(this);
        }
    }
}
