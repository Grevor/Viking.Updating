using System.Collections.Generic;

namespace Viking.Updating
{
    public interface ITriggerer<TTrigger>
    {
        void Trigger(IEnumerable<TTrigger> triggers);
        void Trigger(params TTrigger[] triggers);
    }
}
