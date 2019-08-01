using System;
using System.Linq;
using Viking.Updating.Pipeline;

namespace Viking.Updating.TestConsole
{
    public class UpdateSignaler : ISignaler
    {
        private UpdateGraph<IPipelineStep> UpdateGraph { get; } = new UpdateGraph<IPipelineStep>();

        public void DeregisterDependency(IPipelineStep upstream, IPipelineStep downstream)
        {
        }
        public void DeregisterDependency(IPipelineStep upstream, Action action)
        {
        }
        public void DeregisterDependency(IPipelineStep upstream, IPipelineStep downstream, IPipelineStep intermediary)
        {
        }

        public void Invalidate(IPipelineStep step) => UpdateGraph.Trigger(new[] { step });

        public void RegisterDependency(IPipelineStep upstream, IPipelineStep downstream)=>
            UpdateGraph.AddUpdate(new[] { upstream }, new UpdateFunction<IPipelineStep>(downstream.Invalidate, downstream.Name), new[] { downstream });
        public void RegisterDependency(IPipelineStep upstream, Action action)=>
            UpdateGraph.AddUpdate(new[] { upstream }, action, Enumerable.Empty<IPipelineStep>());

        public void RegisterDependency(IPipelineStep upstream, IPipelineStep downstream, IPipelineStep intermediary) =>
            UpdateGraph.AddUpdate(new[] { upstream }, new UpdateFunction<IPipelineStep>(intermediary.Invalidate, intermediary.Name), new[] { downstream });
    }
}
