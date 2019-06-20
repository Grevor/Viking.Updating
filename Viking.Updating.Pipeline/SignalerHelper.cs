using System;
using System.Linq;

namespace Viking.Updating.Pipeline
{
    internal static class SignalerHelper
    {
        public static void AssertSameSignaler(params ISignaler[] signalers)
        {
            if (signalers.Length <= 0)
                return;
            var b = signalers[0];
            if (!signalers.All(s => s == b))
                throw new ArgumentException("Not all signalers were the same.");
        }

        public static void RegisterMultiple(this ISignaler signaler, IPipelineStep downstream, params IPipelineStep[] upstreams)
        {
            AssertSameSignaler(upstreams.Select(u => u.Signaler).ToArray());
            foreach (var up in upstreams)
                signaler.RegisterDependency(up, downstream);
        }
    }
}
