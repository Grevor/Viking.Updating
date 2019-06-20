using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Viking.Updating.Tests
{
    [TestFixture(TestName ="UpdateGraph tests", TestOf =typeof(UpdateGraph<>))]
    public class UpdateGraphTests
    {
        [Test]
        public void InitialGraphIsValid()
        {
            var sut = new UpdateGraph<int>();
            sut.ValidateGraph();
        }

        [TestCase("first:a second:a", "a", "first:second", TestName ="Two different updaters with the same trigger will both be triggered once by that trigger")]
        [TestCase("first:ab", "ab", "first", TestName ="Same updater registered to two triggers will only be triggered once despite both triggers triggering")]
        [TestCase("first:a first:b", "ab", "first", TestName = "Same updater registered to two triggers (by two different calls) will only be triggered once despite both triggers triggering")]
        [TestCase("first:a first:b", "a", "first", TestName ="Same updater registered to two triggers (by two different calls) will be triggered by older defined triggers")]
        [TestCase("first:a first:b", "b", "first", TestName ="Same updater registered to two triggers (by two different calls) will be triggered by newly defined triggers")]

        [TestCase("first:a:b second:b", "b", "second", TestName = "Updates does not propagate up the chain")]
        [TestCase("first:a:c second:b:c following:c", "a", "first following", TestName = "Updates propagate down the chain and triggers updates on downstream updaters, but not on updaters off that path")]
        [TestCase("first:a:b second:b:c third:c:d fourth:d", "a", "first second third fourth", TestName = "Updates propagate down the chain in order")]
        [TestCase("first:a:c second:b:c third:c", "ab", "first:second third", TestName = "Multiple updates propagate down the chain but still results in only one update")]

        [TestCase("first:a:b second:b:c: third:c", "a", "first second", TestName = "Updates that are not triggered will cause update to not run")]

        [TestCase("first:a:b second:b:a actual:c:d continued:d:e", "c", "actual continued", TestName = "Cycles that are not part of currently run update graph are not discovered")]
        public void HappyCaseTests(string graphDefinition, string triggers, string expectedUpdateOrder)
        {
            var graph = new UpdateGraph<string>();
            var allSteps = GetUpdateGraphFromString(graph, graphDefinition);
            var triggs = triggers.Select(c => c.ToString());

            graph.Trigger(triggs);

            AssertGraph(allSteps, expectedUpdateOrder.Split(' ').Select(s => s.Split(':')));
        }



        [TestCase("first:a:b second:b:a", "a", TestName = "Simple 2-cycle is found on trigger")]
        [TestCase("first:a:b second:b:c third:c:d fourth:d:e fifth:e:a", "a", TestName = "5-cycle is found on trigger")]

        [TestCase("first:a:b second:b:c third:c:b", "a", TestName = "2-cycle is found down the update list")]
        public void SadCaseTests(string graphDefinition, string triggers)
        {
            var graph = new UpdateGraph<string>();
            var allSteps = GetUpdateGraphFromString(graph, graphDefinition);
            var triggs = triggers.Select(c => c.ToString());

            Assert.Throws<UpdateGraphException>(() => graph.Trigger(triggs));
        }


        private void AssertGraph(Dictionary<string, UpdateStep> allSteps, IEnumerable<string[]> order)
        {
            var seen = new List<UpdateStep>();
            foreach(var group in order)
            {
                foreach(var step in group.Select(g=>allSteps[g]))
                {
                    Assert.AreEqual(1, step.Updates.Count, $"Step '{step.Name}' has {step.Updates.Count} updates. Expected exactly 1.");
                    foreach (var seenStep in seen)
                        Assert.Greater(step.Updates[0], seenStep.Updates[0], $"Step '{step.Name}' was encountered earlier than step '{seenStep.Name}'. Expected the reverse order.");
                }
                seen.AddRange(group.Select(g => allSteps[g]));
            }

            foreach (var step in allSteps.Values.Except(seen))
                Assert.AreEqual(0, step.Updates.Count, $"Step '{step.Name}' has {step.Updates.Count} updates. Expected exactly 0.");
        }

        private Dictionary<string, UpdateStep> GetUpdateGraphFromString(UpdateGraph<string> graph, string str)
        {
            var dictionary = new Dictionary<string, UpdateStep>();
            var provider = new UpdateOrderProvider();
            var updateNodes = str.Split(' ');

            foreach(var node in updateNodes)
            {
                (var name, var triggering, var triggered, var actual) = ParseUpdateStep(node);

                if(!dictionary.TryGetValue(name, out var step))
                {
                    step = new UpdateStep(provider, name);
                    dictionary.Add(name, step);
                }
                foreach (var trigger in actual)
                    step.Triggers.Add(trigger);
                graph.AddUpdate(triggering, new UpdateFunction<string>(step.Update, name), triggered);
            }

            return dictionary;
        }

        private (string name, IEnumerable<string> triggering, IEnumerable<string> triggered, IEnumerable<string> actualTriggers) ParseUpdateStep(string str)
        {
            var split = str.Split(':');
            var name = split[0];
            var triggering = split[1].Select(c => c.ToString());
            var triggered = split.Length >= 3 ? split[2].Select(c => c.ToString()) : Enumerable.Empty<string>();
            var actualTriggers = split.Length >= 4 ? split[3].Select(c => c.ToString()) : triggered;

            return (name, triggering, triggered, actualTriggers);
        }

        private class UpdateStep
        {
            public List<int> Updates { get; } = new List<int>();
            public UpdateOrderProvider Provider { get; }
            public string Name { get; }
            public HashSet<string> Triggers { get; } = new HashSet<string>();

            public UpdateStep(UpdateOrderProvider provider, string name)
            {
                Provider = provider;
                Name = name;
            }

            public void Update(IUpdateChain<string> chain)
            {
                Updates.Add(Provider.GetUpdateNumber());
                chain.Trigger(Triggers);
            }
        }

        private class UpdateOrderProvider
        {
            private int Update { get; set; }
            public int GetUpdateNumber() => Update++;
        }
            
    }
}
