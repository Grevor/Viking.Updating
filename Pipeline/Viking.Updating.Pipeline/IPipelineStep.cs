namespace Viking.Updating.Pipeline
{
    public interface IPipelineStep
    {
        string Name { get; }
        ISignaler Signaler { get; }

        void Invalidate();
    }

    public interface IProvider<TItem>
    {
        TItem GetItem();
    }

    public interface IPipelineStep<TItem> : IPipelineStep, IProvider<TItem>
    {

    }
}
