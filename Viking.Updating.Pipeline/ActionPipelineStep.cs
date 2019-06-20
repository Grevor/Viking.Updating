using System;

namespace Viking.Updating.Pipeline
{
    public class ActionPipelineStep<TOutput> : IPipelineStep<TOutput>
    {
        public ActionPipelineStep(string name, IPipelineStep<TOutput> input, Action action) : this(name, input, _ => action()) { }
        public ActionPipelineStep(string name, IPipelineStep<TOutput> input, Action<TOutput> action)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Signaler.RegisterMultiple(this, Input);
        }

        public string Name { get; }
        public IPipelineStep<TOutput> Input { get; }
        public Action<TOutput> Action { get; }

        public ISignaler Signaler => Input.Signaler;

        public TOutput GetItem() => Input.GetItem();

        public void Invalidate() => Action(GetItem());
    }

    public class ActionPipelineStep<TOutput1, TOutput2> : IPipelineStep<TOutput1>
    {
        public ActionPipelineStep(string name, IPipelineStep<TOutput1> input1, IPipelineStep<TOutput2> input2, Action action) : this(name, input1, input2, (a,b) => action()) { }
        public ActionPipelineStep(string name, IPipelineStep<TOutput1> input1, IPipelineStep<TOutput2> input2, Action<TOutput1, TOutput2> action)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Input1 = input1 ?? throw new ArgumentNullException(nameof(input1));
            Input2 = input2 ?? throw new ArgumentNullException(nameof(input2));
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Signaler.RegisterMultiple(this, Input1, Input2);
        }

        public string Name { get; }
        public IPipelineStep<TOutput1> Input1 { get; }
        public IPipelineStep<TOutput2> Input2 { get; }
        public Action<TOutput1, TOutput2> Action { get; }

        public ISignaler Signaler => Input1.Signaler;

        public TOutput1 GetItem() => Input1.GetItem();

        public void Invalidate() => Action(Input1.GetItem(), Input2.GetItem());
    }
}
