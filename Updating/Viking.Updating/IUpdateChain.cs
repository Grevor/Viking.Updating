using System.Collections.Generic;

namespace Viking.Updating
{
    public interface IUpdateChain<TTrigger> : ITriggerer<TTrigger>
    {
        IEnumerable<TTrigger> TriggeringTriggers { get; }

        void BeginAccumulatingUpdates();
        void EndAccumulatingUpdates();
    }
}
