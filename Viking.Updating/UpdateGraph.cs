using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Viking.Updating
{
    public class UpdateGraph<TTrigger> : ITriggerer<TTrigger>
    {
        private OneToManyDictionary<TTrigger, UpdateNode<TTrigger>, HashSet<UpdateNode<TTrigger>>> TriggerToTriggeredNodesDictionary { get; }
            = new OneToManyDictionary<TTrigger, UpdateNode<TTrigger>, HashSet<UpdateNode<TTrigger>>>(() => new HashSet<UpdateNode<TTrigger>>());
        private OneToManyDictionary<TTrigger, UpdateNode<TTrigger>, HashSet<UpdateNode<TTrigger>>> TriggerToTriggeringNodesDictionary { get; }
            = new OneToManyDictionary<TTrigger, UpdateNode<TTrigger>, HashSet<UpdateNode<TTrigger>>>(() => new HashSet<UpdateNode<TTrigger>>());

        private Dictionary<Updater<TTrigger>, UpdateNode<TTrigger>> UpdaterToNodeDictionary { get; } = new Dictionary<Updater<TTrigger>, UpdateNode<TTrigger>>();

        private bool IsUpdating => CurrentUpdateNode != null;
        private HashSet<TTrigger> TriggeredUpdatesSinceStart { get; } = new HashSet<TTrigger>();
        private UpdateNode<TTrigger> CurrentUpdateNode { get; set; }
        private List<UpdateNode<TTrigger>> RunUpdateNodes { get; } = new List<UpdateNode<TTrigger>>();

        private HashSet<TTrigger> AccumulatedTriggers { get; } = new HashSet<TTrigger>();
        private int AccumulationRequests { get; set; }

        #region Update Helpers
        private void StartUpdateCycle(IEnumerable<TTrigger> triggers)
        {
            foreach (var trigger in triggers)
                TriggeredUpdatesSinceStart.Add(trigger);
        }
        private void CleanUpUpdateCycle()
        {
            TriggeredUpdatesSinceStart.Clear();
            RunUpdateNodes.Clear();
            CurrentUpdateNode = null;
        }
        private void SetCurrentNode(UpdateNode<TTrigger> node)
        {
            CurrentUpdateNode = node;
            RunUpdateNodes.Add(node);
        }

        private void RunUpdates(List<UpdateNode<TTrigger>> sortedNodes)
        {
            var triggers = new UpdateChain(this);
            foreach(var node in sortedNodes)
                RunUpdate(node, triggers);
        }
        private void RunUpdate(UpdateNode<TTrigger> node, UpdateChain chain)
        {
            var triggers = chain.CurrentTriggers;
            triggers.Clear();
            triggers.UnionWith(node.IncommingTriggers);
            triggers.IntersectWith(TriggeredUpdatesSinceStart);

            if (triggers.Count <= 0)
                return;

            try
            {
                SetCurrentNode(node);
                node.Function.Invoke(chain);
            }
            catch(Exception e)
            {
                var messageBuilder = new StringBuilder();
                messageBuilder
                    .Append(CurrentNodeExceptionHeader)
                    .AppendLine(" threw an exception:")
                    .AppendLine(e.Message)
                    .AppendLine()
                    .Append(UpdateChainAsString);
                ThrowUpdateError(new UpdateGraphException(messageBuilder.ToString(), e));
            }
        }

        private void VerifyTriggersAgainstCurrentNodeAndAddThemAsTriggered(IEnumerable<TTrigger> triggers)
        {
            if(!triggers.All(CurrentUpdateNode.OutgoingTriggers.Contains))
            {
                var messageBuilder = new StringBuilder();
                messageBuilder
                    .Append(CurrentNodeExceptionHeader)
                    .Append(" triggered one or more triggers it was not supposed to (")
                    .Append(string.Join(",", triggers))
                    .AppendLine("). Update chain compromised.")
                    .AppendLine()
                    .Append(UpdateChainAsString);
                ThrowUpdateError(new UpdateGraphException(messageBuilder.ToString()));
            }
            else
            {
                foreach (var trigger in triggers)
                    TriggeredUpdatesSinceStart.Add(trigger);
            }
        }
        private string UpdateChainAsString => FormattableString.Invariant($"## Update chain thus far ##{Environment.NewLine}{string.Join(Environment.NewLine, RunUpdateNodes)}");
        private string CurrentNodeExceptionHeader => CurrentUpdateNode == null ? "<No Update Node>" : CurrentUpdateNode.ToString();

        private void Accumulate(IEnumerable<TTrigger> triggers)
        {
            foreach (var trigger in triggers)
                AccumulatedTriggers.Add(trigger);
        }

        private void FindCycleAndThrow(IEnumerable<TTrigger> triggers)
        {
            var cycles = FindStronglyConnectedComponents(triggers);
            if (!cycles.Any())
                return;

            var messageBuilder = new StringBuilder()
                .Append("Found cycle(s) in update graph:");

            var index = 1;
            foreach(var cycle in cycles)
            {
                messageBuilder
                    .AppendLine()
                    .AppendLine(FormattableString.Invariant($"## Cycle {index} ##"))
                    .AppendLine(string.Join(Environment.NewLine, cycle));
            }
            ThrowUpdateError(new UpdateGraphException(messageBuilder.ToString()));
        }

        #endregion

        #region Exception Handling
        private void ThrowUpdateError(Exception e)
        {
            CleanUpUpdateCycle();
            throw e;
        }
        #endregion

        #region Public Interface

        public void AddUpdate(IEnumerable<TTrigger> incomming, Updater function, IEnumerable<TTrigger> potentialTriggers) =>
            AddUpdate(incomming, (UpdateFunction<TTrigger>)function, potentialTriggers);
        public void AddUpdate(IEnumerable<TTrigger> incomming, Updater<TTrigger> function, IEnumerable<TTrigger> potentialTriggers) =>
            AddUpdate(incomming, (UpdateFunction<TTrigger>)function, potentialTriggers);
        public void AddUpdate(IEnumerable<TTrigger> incomming, UpdateFunction<TTrigger> function, IEnumerable<TTrigger> potentialTriggers)
        {
            if(!UpdaterToNodeDictionary.TryGetValue(function.Updater, out var node))
            {
                node = new UpdateNode<TTrigger>(function);
                UpdaterToNodeDictionary.Add(function.Updater, node);
            }

            node.NodeRegisterCount++;
            node.AddName(function.Name);
            foreach (var inc in incomming)
                if (node.AddIncomming(inc))
                    TriggerToTriggeredNodesDictionary.Add(inc, node);

            foreach (var inc in potentialTriggers)
                if (node.AddOutgoing(inc))
                    TriggerToTriggeringNodesDictionary.Add(inc, node);
        }

        public void RemoveUpdate(Updater function) => RemoveUpdate((UpdateFunction<TTrigger>)function);
        public void RemoveUpdate(Updater<TTrigger> function) => RemoveUpdate((UpdateFunction<TTrigger>)function);
        public void RemoveUpdate(UpdateFunction<TTrigger> function)
        {
            if (!UpdaterToNodeDictionary.TryGetValue(function.Updater, out var node))
                return;

            node.NodeRegisterCount--;
            if (node.NodeRegisterCount > 0)
                return;

            foreach (var trigger in node.IncommingTriggers)
                TriggerToTriggeredNodesDictionary.Remove(trigger, node);
            foreach (var trigger in node.OutgoingTriggers)
                TriggerToTriggeringNodesDictionary.Remove(trigger, node);
        }

        public void Trigger(params TTrigger[] triggers) => Trigger((IEnumerable<TTrigger>)triggers);
        public void Trigger(IEnumerable<TTrigger> triggers)
        {
            if (IsUpdating)
            {
                VerifyTriggersAgainstCurrentNodeAndAddThemAsTriggered(triggers);
                return;
            }

            if(AccumulationRequests > 0)
            {
                Accumulate(triggers);
                return;
            }

            var sortedNodes = GetTopologySortedNodes(triggers);
            if (sortedNodes == null)
            {
                FindCycleAndThrow(triggers);
                return;
            }

            StartUpdateCycle(triggers);
            RunUpdates(sortedNodes);
            CleanUpUpdateCycle();
        }

        public void ValidateGraph() => FindCycleAndThrow(TriggerToTriggeredNodesDictionary.Keys);

        public void BeginAccumulatingUpdates() => AccumulationRequests++;
        public void EndAccumulatingUpdates()
        {
            AccumulationRequests--;
            if(AccumulationRequests <= 0)
            {
                var triggers = AccumulatedTriggers.ToList();
                AccumulatedTriggers.Clear();
                Trigger(triggers);
            }
        }

        #endregion


        #region Topology Sort
        private List<UpdateNode<TTrigger>> GetTopologySortedNodes(IEnumerable<TTrigger> triggers)
        {
            foreach (var nodes in UpdaterToNodeDictionary.Values)
                nodes.Mark = Mark.NoMark;
            var initialNodes = GetTriggeredNodes(triggers).ToList();
            var result = new List<UpdateNode<TTrigger>>();

            for (int i = 0; i < initialNodes.Count; ++i)
            {
                var node = initialNodes[i];
                if (node.Mark == Mark.NoMark)
                    if (Visit(result, node))
                        return null;
            }

            result.Reverse();
            return result;
        }
        private bool Visit(List<UpdateNode<TTrigger>> L, UpdateNode<TTrigger> n)
        {
            if (n.Mark == Mark.PermanentMark) return false;
            if (n.Mark == Mark.TemporaryMark) return true; // Cycle found

            n.Mark = Mark.TemporaryMark;
            foreach (var m in GetTriggeredNodes(n.OutgoingTriggers))
                if (Visit(L, m))
                    return true;

            n.Mark = Mark.PermanentMark;
            L.Add(n);
            return false;
        }
        #endregion

        #region Tarjan
        private IEnumerable<IEnumerable<UpdateNode<TTrigger>>> FindStronglyConnectedComponents(IEnumerable<TTrigger> triggers)
        {
            var startNodes = GetTriggeredNodes(triggers);

            var SCC = new List<IEnumerable<UpdateNode<TTrigger>>>();
            var S = new Stack<TarjanNode<TTrigger>>();
            var tarjanInfo = new Dictionary<UpdateNode<TTrigger>, TarjanNode<TTrigger>>();
            var index = 0;

            foreach (var node in startNodes)
            {
                if(!tarjanInfo.TryGetValue(node, out var v))
                {
                    v = new TarjanNode<TTrigger>(node);
                    tarjanInfo.Add(node, v);
                    StrongConnect(ref index, S, tarjanInfo, SCC, v);
                }
            }

            return SCC;
        }

        private void StrongConnect(
            ref int index,
            Stack<TarjanNode<TTrigger>> S,
            Dictionary<UpdateNode<TTrigger>, TarjanNode<TTrigger>> tarjanInfo,
            List<IEnumerable<UpdateNode<TTrigger>>> SCC,
            TarjanNode<TTrigger> v)
        {
            v.Index = index;
            v.LowLink = index;
            index++;
            S.Push(v);
            v.OnStack = true;

            foreach (var node in GetTriggeredNodes(v.Node.OutgoingTriggers))
            {
                if (!tarjanInfo.TryGetValue(node, out var w))
                {
                    w = new TarjanNode<TTrigger>(node);
                    tarjanInfo.Add(node, w);
                    StrongConnect(ref index, S, tarjanInfo, SCC, w);
                    v.LowLink = Math.Min(w.LowLink, v.LowLink);
                }
                else if (w.OnStack)
                {
                    v.LowLink = Math.Min(v.LowLink, w.Index);
                }
            }

            if (v.LowLink == v.Index)
            {
                var cycle = new List<UpdateNode<TTrigger>>();
                while (S.Peek() != v)
                {
                    var w = S.Pop();
                    w.OnStack = false;
                    cycle.Add(w.Node);
                }
                cycle.Add(S.Pop().Node);
                if (cycle.Count > 1)
                    SCC.Add(cycle);
            }
        }



        #endregion

        private IEnumerable<UpdateNode<TTrigger>> GetTriggeredNodes(IEnumerable<TTrigger> triggers) => triggers.SelectMany(GetTriggeredNodes).Distinct();
        private IEnumerable<UpdateNode<TTrigger>> GetTriggeredNodes(TTrigger trigger)
        {
            if (TriggerToTriggeredNodesDictionary.TryGetValue(trigger, out var nodes))
                return nodes;
            else
                return Enumerable.Empty<UpdateNode<TTrigger>>();
        }

        private class UpdateChain : IUpdateChain<TTrigger>
        {
            public UpdateChain(UpdateGraph<TTrigger> graph) => Graph = graph;

            public UpdateGraph<TTrigger> Graph { get; }

            public HashSet<TTrigger> CurrentTriggers { get; } = new HashSet<TTrigger>();
            public IEnumerable<TTrigger> TriggeringTriggers => CurrentTriggers;

            public void BeginAccumulatingUpdates() => Graph.BeginAccumulatingUpdates();
            public void EndAccumulatingUpdates() => Graph.EndAccumulatingUpdates();

            public void Trigger(IEnumerable<TTrigger> triggers) => Graph.Trigger(triggers);
            public void Trigger(params TTrigger[] triggers) => Graph.Trigger(triggers);
        }
    }
}
