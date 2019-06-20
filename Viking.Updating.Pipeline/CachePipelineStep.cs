using System;
using System.Collections.Generic;
using System.Text;

namespace Viking.Updating.Pipeline
{
    public class CachePipelineStep<TItem> : IPipelineStep<TItem>
    {
        public CachePipelineStep(IPipelineStep<TItem> step)
        {
            Step = step ?? throw new ArgumentNullException(nameof(step));
            Name = $"Cache for step '{step.Name}'";
            Signaler = Step.Signaler;

            Signaler.RegisterDependency(Step, this);
        }

        public IPipelineStep<TItem> Step { get; }
        public string Name { get; }
        public ISignaler Signaler { get; }

        public TItem CachedValue { get; private set; }
        public bool CacheIsValid { get; private set; }

        public TItem GetItem()
        {
            if (!CacheIsValid)
                CacheNewValue();
            return CachedValue;
        }

        private void CacheNewValue()
        {
            CachedValue = Step.GetItem();
            CacheIsValid = true;
        }

        public void Invalidate()
        {
            CacheIsValid = false;
            CachedValue = default;
            Signaler.Invalidate(this);
        }
    }
}
