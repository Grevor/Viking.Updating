using System;

namespace Viking.Updating.Pipeline
{
    public interface ISignaler
    {
        void RegisterDependency(IPipelineStep upstream, IPipelineStep downstream);
        void DeregisterDependency(IPipelineStep upstream, IPipelineStep downstream);

        void RegisterDependency(IPipelineStep upstream, IPipelineStep downstream, IPipelineStep intermediary);
        void DeregisterDependency(IPipelineStep upstream, IPipelineStep downstream, IPipelineStep intermediary);

        void RegisterDependency(IPipelineStep upstream, Action action);
        void DeregisterDependency(IPipelineStep upstream, Action action);

        void Invalidate(IPipelineStep step);
    }
}
