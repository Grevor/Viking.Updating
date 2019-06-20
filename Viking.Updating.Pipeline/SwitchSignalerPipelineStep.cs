using System;

namespace Viking.Updating.Pipeline
{
    public class SwitchSignalerPipelineStep<TOutput> : IPipelineStep<TOutput>
    {
        public SwitchSignalerPipelineStep(IPipelineStep<TOutput> upstream, ISignaler signaler)
        {
            Upstream = upstream ?? throw new ArgumentNullException(nameof(upstream));
            Signaler = signaler ?? throw new ArgumentNullException(nameof(signaler));
            Signaler.RegisterDependency(Upstream, this);
        }

        public IPipelineStep<TOutput> Upstream { get; }
        public ISignaler Signaler { get; }

        public string Name => $"Signaler switcher for step '{Upstream.Name}'";

        public TOutput GetItem() => Upstream.GetItem();

        public void Invalidate() => Signaler.Invalidate(this);
    }
}
