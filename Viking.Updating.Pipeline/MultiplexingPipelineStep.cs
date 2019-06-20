using System;
using System.Collections.Generic;

namespace Viking.Updating.Pipeline
{
    public class MultiplexingPipelineStep<TOutput, TSelect> : IPipelineStep<TOutput>
    {
        private Dictionary<TSelect, PausePipelineStep<TOutput>> Inputs { get; } = new Dictionary<TSelect, PausePipelineStep<TOutput>>();
        public string Name { get; }
        public IPipelineStep<TSelect> Select { get; }
        public ISignaler Signaler { get; }

        private TOutput Output => Inputs[Select.GetItem()].GetItem();

        public MultiplexingPipelineStep(string name, IPipelineStep<TSelect> select)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Select = select ?? throw new ArgumentNullException(nameof(select));
            Signaler = select.Signaler;
        }

        public void AddInput(TSelect select, IPipelineStep<TOutput> input)
        {
            SignalerHelper.AssertSameSignaler(Signaler, input.Signaler);

            var pauseStep = new PausePipelineStep<TOutput>($"Multiplexer '{Name}' - Branch {select}", input);
            pauseStep.Pause();
            Inputs.Add(select, pauseStep);

            Signaler.RegisterDependency(input, this);
        }

        public void Invalidate()
        {
            if (Inputs.TryGetValue(Select.GetItem(), out var input))
            {
                var mustInvalidate = input.HasPendingInvalidate;

                input.Unpause(false);
                input.Pause();

                if (mustInvalidate)
                    Signaler.Invalidate(this);
            }
        }

        public TOutput GetItem() => Output;
    }
}
