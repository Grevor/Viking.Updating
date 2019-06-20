using System;

namespace Viking.Updating.Pipeline
{
    public class PausePipelineStep<TOutput> : IPipelineStep<TOutput>
    {
        public PausePipelineStep(string name, IPipelineStep<TOutput> input)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Signaler = Input.Signaler;

            Signaler.RegisterDependency(Input, this);
        }

        public string Name { get; }
        public IPipelineStep<TOutput> Input { get; }
        public ISignaler Signaler { get; }

        public bool HasPendingInvalidate { get; private set; }
        public bool IsPaused { get; private set; }

        public TOutput GetItem() => Input.GetItem();

        public void Pause() => IsPaused = true;
        public void Unpause(bool invalidateIfPending)
        {
            if (!IsPaused)
                return;
            var mustInvalidate = invalidateIfPending && HasPendingInvalidate;
            IsPaused = false;
            HasPendingInvalidate = false;
            if (mustInvalidate)
                Invalidate();
        }

        public void Invalidate()
        {
            if (IsPaused)
                HasPendingInvalidate = true;
            else
                Signaler.Invalidate(this);
        }
    }
}
