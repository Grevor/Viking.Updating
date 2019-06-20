using System;

namespace Viking.Updating.Pipeline
{
    public class ConstantPipelineStep<TItem> : IPipelineStep<TItem>
    {
        public ConstantPipelineStep(string name, TItem constant, ISignaler signaler)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Constant = constant;
            Signaler = signaler ?? throw new ArgumentNullException(nameof(signaler));
        }

        public string Name { get; }
        public TItem Constant { get; }
        public ISignaler Signaler { get; }

        public TItem GetItem() => Constant;

        public void Invalidate() => Signaler.Invalidate(this);
    }
}
