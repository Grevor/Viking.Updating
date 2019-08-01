using System;
using System.Collections.Generic;

namespace Viking.Updating.Pipeline
{
    public class DemultiplexingPipelineStep<TOutput, TSelect> : IPipelineStep<TOutput>
    {
        private Dictionary<TSelect, IPipelineStep<TOutput>> Outputs { get; }
        public string Name { get; }
        public IPipelineStep<TOutput> Input { get; }
        public IPipelineStep<TSelect> Select { get; }
        public ISignaler Signaler { get; }

        public DemultiplexingPipelineStep(string name, IPipelineStep<TOutput> input, IPipelineStep<TSelect> select)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Select = select ?? throw new ArgumentNullException(nameof(select));
            Signaler = Input.Signaler;
            Signaler.RegisterMultiple(this, Input, Select);
            Outputs = new Dictionary<TSelect, IPipelineStep<TOutput>>();
        }

        public IPipelineStep<TOutput> GetBranch(TSelect select)
        {
            if(!Outputs.TryGetValue(select, out var branch))
            {
                branch = new PassThroughPipelineStep<TOutput>($"Demultiplexing step {Name} - Branch for select value {select}", this, true, false);
                Signaler.RegisterDependency(Input, branch, this);
                Outputs.Add(select, branch);
            }
            return branch;
        }

        public void Invalidate()
        {
            Signaler.Invalidate(this); // To trigger potential registers for this step (though you should really not be doing this).
            if (Outputs.TryGetValue(Select.GetItem(), out var output))
                output.Invalidate();
        }

        public TOutput GetItem() => Input.GetItem();
    }
}
