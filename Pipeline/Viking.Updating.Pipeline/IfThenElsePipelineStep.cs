namespace Viking.Updating.Pipeline
{
    public class IfThenElsePipelineStep<TOutput> : DemultiplexingPipelineStep<TOutput, bool>
    {
        public IfThenElsePipelineStep(string name, IPipelineStep<TOutput> input, IPipelineStep<bool> condition) : base(name, input, condition)
        {
            TrueBranch = GetBranch(true);
            FalseBranch = GetBranch(false);
        }
        public IPipelineStep<TOutput> TrueBranch { get; }
        public IPipelineStep<TOutput> FalseBranch { get; }
    }
}
