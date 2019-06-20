using System;
using System.Collections.Generic;

namespace Viking.Updating.Pipeline
{
    public class AssignableValuePipelineStep<TItem> : IPipelineStep<TItem>
    {
        public AssignableValuePipelineStep(string name, TItem initial, ISignaler signaler) : this(name,initial, signaler, EqualityComparer<TItem>.Default) { }
        public AssignableValuePipelineStep(string name, TItem initial, ISignaler signaler, IEqualityComparer<TItem> comparer)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = initial;
            Signaler = signaler ?? throw new ArgumentNullException(nameof(signaler));
            Comparer = comparer;
        }

        public string Name { get; }
        public TItem Value { get; private set; }
        public ISignaler Signaler { get; }
        public IEqualityComparer<TItem> Comparer { get; }

        public TItem GetItem() => Value;
        public void Set(TItem value)
        {
            var shouldSignal = !Comparer.Equals(Value, value);
            Value = value;
            if (shouldSignal)
                Invalidate();
        }

        public void Invalidate() => Signaler.Invalidate(this);
    }
}
